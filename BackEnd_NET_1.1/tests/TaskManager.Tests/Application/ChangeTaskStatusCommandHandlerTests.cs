using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.UseCases.Handlers.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class ChangeTaskStatusCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryTaskRepository _tasks;
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly TaskItem _pendingTask;

    public ChangeTaskStatusCommandHandlerTests()
    {
        _tasks = new InMemoryTaskRepository(_users);

        var user = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);
        _users.Add(user);

        _pendingTask = TaskItem.Create("Título", user.Id, null, FixedTimeProvider.DefaultUtcNow);
        _tasks.Add(_pendingTask);
    }

    private ChangeTaskStatusCommandHandler CreateHandler() => new(_tasks, _tasks, _unitOfWork, new FixedTimeProvider());

    [Fact]
    public async Task HandleAsync_AllowedTransition_SavesAndReturnsAllowedTransitions()
    {
        var result = await CreateHandler().HandleAsync(
            new ChangeTaskStatusCommand(_pendingTask.Id, "inprogress"),
            CancellationToken.None);

        Assert.Equal(nameof(TaskItemStatusEnum.InProgress), result.Status);
        Assert.Equal([nameof(TaskItemStatusEnum.Pending), nameof(TaskItemStatusEnum.Done)], result.AllowedTransitions);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_PendingToDone_ThrowsWithoutSaving()
    {
        await Assert.ThrowsAsync<InvalidTaskStatusTransitionException>(() => CreateHandler().HandleAsync(
            new ChangeTaskStatusCommand(_pendingTask.Id, nameof(TaskItemStatusEnum.Done)),
            CancellationToken.None));

        Assert.Equal(TaskItemStatusEnum.Pending, _pendingTask.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownTask_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().HandleAsync(
            new ChangeTaskStatusCommand(Guid.NewGuid(), nameof(TaskItemStatusEnum.InProgress)),
            CancellationToken.None));
    }
}
