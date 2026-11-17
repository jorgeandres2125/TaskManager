using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Domain.Entities.Tasks;

namespace TaskManager.Infrastructure.Persistence.Converters;

/// <summary>
/// Formato JSON de la columna Tasks.AdditionalInfo. Las rutas que usan las consultas SQL
/// ($.priority, $.estimatedCompletionDate, $.tags, $.metadata) dependen de estos nombres en camelCase.
/// </summary>
internal static class TaskAdditionalInfoJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string? Serialize(TaskAdditionalInfo? info) =>
        info is null ? null : JsonSerializer.Serialize(info, Options);

    public static TaskAdditionalInfo? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<TaskAdditionalInfo>(json, Options);

    public static string SerializeFragment<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? DeserializeFragment<T>(string? json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
}
