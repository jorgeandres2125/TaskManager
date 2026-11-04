using TaskManager.Application.Enums.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.UseCases.Handlers.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class PatchTaskAdditionalInfoCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryTaskRepository _tasks;
    private readonly RecordingTaskAdditionalInfoWriter _writer = new();
    private readonly TaskItem _task;

    public PatchTaskAdditionalInfoCommandHandlerTests()
    {
        _tasks = new InMemoryTaskRepository(_users);

        var user = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);
        _users.Add(user);

        _task = TaskItem.Create("Título", user.Id, null, FixedTimeProvider.DefaultUtcNow);
        _tasks.Add(_task);
    }

    private PatchTaskAdditionalInfoCommandHandler CreateHandler() => new(_writer, _tasks, new FixedTimeProvider());

    private PatchTaskAdditionalInfoCommand Command(
        Guid? taskId = null,
        string? priority = null,
        IReadOnlyList<string>? addTags = null,
        IReadOnlyList<string>? removeMetadata = null) =>
        new(taskId ?? _task.Id, priority, false, null, false, addTags, null, null, removeMetadata);

    [Fact]
    public async Task HandleAsync_SendsNormalizedPatchAndReturnsTask()
    {
        var result = await CreateHandler().HandleAsync(
            Command(priority: "High", addTags: [" demo ", "DEMO", " ", "api"], removeMetadata: ["ticket", "ticket"]),
            CancellationToken.None);

        var patch = _writer.LastPatch!;
        Assert.Equal(_task.Id, patch.TaskId);
        Assert.Equal(TaskPriorityEnum.High, patch.Priority);
        Assert.Equal(["demo", "api"], patch.AddTags);
        Assert.Empty(patch.RemoveTags);
        Assert.Empty(patch.SetMetadata);
        Assert.Equal(["ticket"], patch.RemoveMetadata);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, patch.UpdatedAt);
        Assert.Equal(_task.Id, result.Id);
    }

    [Fact]
    public async Task HandleAsync_UnknownTask_ThrowsNotFound()
    {
        _writer.Result = TaskAdditionalInfoPatchResultEnum.TaskNotFound;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().HandleAsync(Command(taskId: Guid.NewGuid(), priority: "Low"), CancellationToken.None));
    }

    [Theory]
    [InlineData(TaskAdditionalInfoPatchResultEnum.TooManyTags, nameof(PatchTaskAdditionalInfoCommand.AddTags))]
    [InlineData(TaskAdditionalInfoPatchResultEnum.TooManyMetadataEntries, nameof(PatchTaskAdditionalInfoCommand.SetMetadata))]
    public async Task HandleAsync_LimitExceededInDatabase_ThrowsValidationError(
        TaskAdditionalInfoPatchResultEnum result,
        string expectedProperty)
    {
        _writer.Result = result;

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() =>
            CreateHandler().HandleAsync(Command(addTags: ["demo"]), CancellationToken.None));

        Assert.Contains(expectedProperty, exception.Errors.Keys);
    }
}
