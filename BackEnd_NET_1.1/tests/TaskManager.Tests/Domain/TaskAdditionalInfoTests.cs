using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Tests.Domain;

public class TaskAdditionalInfoTests
{
    [Fact]
    public void Create_WithoutData_ReturnsNull()
    {
        Assert.Null(TaskAdditionalInfo.Create(null, null, ["  ", ""], new Dictionary<string, string>()));
    }

    [Fact]
    public void Create_NormalizesTags()
    {
        var info = TaskAdditionalInfo.Create(TaskPriorityEnum.High, null, [" backend ", "Backend", "", "api"], null);

        Assert.NotNull(info);
        Assert.Equal(["backend", "api"], info.Tags);
        Assert.Equal(TaskPriorityEnum.High, info.Priority);
    }
}
