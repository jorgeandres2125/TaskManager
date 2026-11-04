using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Models.Tasks;

/// <summary>
/// Cambios parciales sobre Tasks.AdditionalInfo, ya normalizados. Solo se tocan las claves indicadas;
/// el resto del JSON se conserva tal cual.
/// </summary>
public record TaskAdditionalInfoPatch(
    Guid TaskId,
    TaskPriorityEnum? Priority,
    bool ClearPriority,
    DateOnly? EstimatedCompletionDate,
    bool ClearEstimatedCompletionDate,
    IReadOnlyList<string> AddTags,
    IReadOnlyList<string> RemoveTags,
    IReadOnlyDictionary<string, string> SetMetadata,
    IReadOnlyList<string> RemoveMetadata,
    DateTime UpdatedAt);
