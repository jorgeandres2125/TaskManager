using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Domain;

public class TaskItemTests
{
    private static readonly DateTime Now = FixedTimeProvider.DefaultUtcNow;

    [Fact]
    public void Create_WithValidData_StartsPendingWithIdAndCreatedAt()
    {
        var userId = Guid.NewGuid();

        var task = TaskItem.Create("  Revisar contrato  ", userId, null, Now);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Revisar contrato", task.Title);
        Assert.Equal(TaskItemStatusEnum.Pending, task.Status);
        Assert.Equal(userId, task.UserId);
        Assert.Equal(Now, task.CreatedAt);
        Assert.Null(task.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_Throws(string title)
    {
        var exception = Assert.Throws<DomainValidationException>(() => TaskItem.Create(title, Guid.NewGuid(), null, Now));

        Assert.Equal(nameof(TaskItem.Title), exception.PropertyName);
    }

    [Fact]
    public void Create_WithoutUser_Throws()
    {
        var exception = Assert.Throws<DomainValidationException>(() => TaskItem.Create("Título", Guid.Empty, null, Now));

        Assert.Equal(nameof(TaskItem.UserId), exception.PropertyName);
    }

    [Fact]
    public void Create_WithNonUtcDate_Throws()
    {
        var localDate = DateTime.SpecifyKind(Now, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => TaskItem.Create("Título", Guid.NewGuid(), null, localDate));
    }

    [Fact]
    public void ChangeStatus_FromPendingToDone_ThrowsAndKeepsStatus()
    {
        var task = TaskItem.Create("Título", Guid.NewGuid(), null, Now);

        var exception = Assert.Throws<InvalidTaskStatusTransitionException>(() => task.ChangeStatus(TaskItemStatusEnum.Done, Now));

        Assert.Equal(TaskItemStatusEnum.Pending, exception.From);
        Assert.Equal(TaskItemStatusEnum.Done, exception.To);
        Assert.Equal(TaskItemStatusEnum.Pending, task.Status);
    }

    [Fact]
    public void ChangeStatus_ThroughInProgress_ReachesDone()
    {
        var task = TaskItem.Create("Título", Guid.NewGuid(), null, Now);
        var later = Now.AddHours(1);

        task.ChangeStatus(TaskItemStatusEnum.InProgress, Now);
        task.ChangeStatus(TaskItemStatusEnum.Done, later);

        Assert.Equal(TaskItemStatusEnum.Done, task.Status);
        Assert.Equal(later, task.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_ToCurrentStatus_DoesNothing()
    {
        var task = TaskItem.Create("Título", Guid.NewGuid(), null, Now);

        task.ChangeStatus(TaskItemStatusEnum.Pending, Now.AddHours(1));

        Assert.Equal(TaskItemStatusEnum.Pending, task.Status);
        Assert.Null(task.UpdatedAt);
    }
}
