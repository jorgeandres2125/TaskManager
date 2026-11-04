using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.UseCases.Handlers.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class UpdateTaskCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryTaskRepository _tasks;
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly User _otherUser = User.Create("Luis", "luis@acme.com", FixedTimeProvider.DefaultUtcNow);
    private readonly TaskItem _task;

    public UpdateTaskCommandHandlerTests()
    {
        _tasks = new InMemoryTaskRepository(_users);

        var user = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);
        _users.Add(user);
        _users.Add(_otherUser);

        _task = TaskItem.Create("Título", user.Id, null, FixedTimeProvider.DefaultUtcNow);
        _task.ChangeStatus(TaskItemStatusEnum.InProgress, FixedTimeProvider.DefaultUtcNow);
        _tasks.Add(_task);
    }

    private UpdateTaskCommandHandler CreateHandler() => new(_tasks, _tasks, _unitOfWork, new FixedTimeProvider());

    [Fact]
    public async Task HandleAsync_ReassignsAndEditsWithoutChangingStatus()
    {
        var result = await CreateHandler().HandleAsync(
            new UpdateTaskCommand(_task.Id, " Nuevo título ", _otherUser.Id, new TaskAdditionalInfoDto("High", null, ["demo"], null)),
            CancellationToken.None);

        Assert.Equal("Nuevo título", result.Title);
        Assert.Equal(_otherUser.Id, result.UserId);
        Assert.Equal("Luis", result.UserName);
        Assert.Equal(nameof(TaskPriorityEnum.High), result.AdditionalInfo?.Priority);
        Assert.Equal(nameof(TaskItemStatusEnum.InProgress), result.Status);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, result.UpdatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WithoutAdditionalInfo_ClearsIt()
    {
        _task.Update(_task.Title, _task.UserId, TaskAdditionalInfo.Create(TaskPriorityEnum.Low, null, null, null), FixedTimeProvider.DefaultUtcNow);

        var result = await CreateHandler().HandleAsync(
            new UpdateTaskCommand(_task.Id, "Título", _task.UserId, null),
            CancellationToken.None);

        Assert.Null(result.AdditionalInfo);
    }

    [Fact]
    public async Task HandleAsync_UnknownTask_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().HandleAsync(
            new UpdateTaskCommand(Guid.NewGuid(), "Título", _otherUser.Id, null),
            CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
