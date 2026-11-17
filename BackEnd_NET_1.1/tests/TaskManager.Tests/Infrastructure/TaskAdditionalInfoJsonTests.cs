using System.Text.Json;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Infrastructure.Persistence.Converters;

namespace TaskManager.Tests.Infrastructure;

public class TaskAdditionalInfoJsonTests
{
    [Fact]
    public void Serialize_UsesThePathsQueriedBySqlServer()
    {
        var info = TaskAdditionalInfo.Create(
            TaskPriorityEnum.High,
            new DateOnly(2026, 11, 20),
            ["backend"],
            new Dictionary<string, string> { ["ticket"] = "DB-42" });

        using var json = JsonDocument.Parse(TaskAdditionalInfoJson.Serialize(info)!);
        var root = json.RootElement;

        Assert.Equal("High", root.GetProperty("priority").GetString());
        Assert.Equal("2026-11-20", root.GetProperty("estimatedCompletionDate").GetString());
        Assert.Equal("backend", root.GetProperty("tags")[0].GetString());
        Assert.Equal("DB-42", root.GetProperty("metadata").GetProperty("ticket").GetString());
    }

    [Fact]
    public void Serialize_ThenDeserialize_PreservesValues()
    {
        var info = TaskAdditionalInfo.Create(TaskPriorityEnum.Low, null, ["a", "b"], null);

        var restored = TaskAdditionalInfoJson.Deserialize(TaskAdditionalInfoJson.Serialize(info));

        Assert.NotNull(restored);
        Assert.Equal(TaskPriorityEnum.Low, restored.Priority);
        Assert.Null(restored.EstimatedCompletionDate);
        Assert.Equal(["a", "b"], restored.Tags);
    }
}
