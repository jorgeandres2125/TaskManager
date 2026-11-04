using TaskManager.Domain.Enums.Tasks;
using TaskManager.Domain.Utilities.Tasks;

namespace TaskManager.Tests.Domain;

public class TaskStatusTransitionsTests
{
    [Theory]
    [InlineData(TaskItemStatusEnum.Pending, TaskItemStatusEnum.InProgress, true)]
    [InlineData(TaskItemStatusEnum.Pending, TaskItemStatusEnum.Done, false)]
    [InlineData(TaskItemStatusEnum.InProgress, TaskItemStatusEnum.Done, true)]
    [InlineData(TaskItemStatusEnum.InProgress, TaskItemStatusEnum.Pending, true)]
    [InlineData(TaskItemStatusEnum.Done, TaskItemStatusEnum.InProgress, true)]
    [InlineData(TaskItemStatusEnum.Done, TaskItemStatusEnum.Pending, false)]
    public void IsAllowed_ReturnsExpectedResult(TaskItemStatusEnum from, TaskItemStatusEnum to, bool expected)
    {
        Assert.Equal(expected, TaskStatusTransitions.IsAllowed(from, to));
    }

    [Fact]
    public void EveryStatus_HasTransitionsDefined()
    {
        foreach (var status in Enum.GetValues<TaskItemStatusEnum>())
        {
            Assert.NotEmpty(TaskStatusTransitions.GetAllowedTargets(status));
        }
    }
}
