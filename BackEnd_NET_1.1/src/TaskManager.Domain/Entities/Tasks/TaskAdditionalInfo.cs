using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Domain.Entities.Tasks;

/// <summary>
/// Información adicional de una tarea. Se persiste como JSON en la columna Tasks.AdditionalInfo
/// (requisito de manejo de JSON nativo en SQL Server).
/// </summary>
public record TaskAdditionalInfo
{
    public const int MaxTags = 10;
    public const int TagMaxLength = 50;
    public const int MaxMetadataEntries = 20;
    public const int MetadataKeyMaxLength = 50;
    public const int MetadataValueMaxLength = 500;

    public TaskPriorityEnum? Priority { get; init; }

    public DateOnly? EstimatedCompletionDate { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public static TaskAdditionalInfo? Create(
        TaskPriorityEnum? priority,
        DateOnly? estimatedCompletionDate,
        IEnumerable<string>? tags,
        IReadOnlyDictionary<string, string>? metadata)
    {
        var normalizedTags = (tags ?? [])
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var normalizedMetadata = (metadata ?? new Dictionary<string, string>())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Key))
            .ToDictionary(entry => entry.Key.Trim(), entry => entry.Value);

        if (priority is null && estimatedCompletionDate is null && normalizedTags.Length == 0 && normalizedMetadata.Count == 0)
        {
            return null;
        }

        return new TaskAdditionalInfo
        {
            Priority = priority,
            EstimatedCompletionDate = estimatedCompletionDate,
            Tags = normalizedTags,
            Metadata = normalizedMetadata,
        };
    }
}
