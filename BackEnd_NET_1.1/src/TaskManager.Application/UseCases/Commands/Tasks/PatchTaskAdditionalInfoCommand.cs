namespace TaskManager.Application.UseCases.Commands.Tasks;

/// <summary>
/// Cambia solo los campos indicados de la información adicional (PATCH), con JSON_MODIFY en SQL Server.
/// Lo que no se indica se conserva. La prioridad y la fecha se quitan con ClearPriority y ClearEstimatedCompletionDate.
/// </summary>
public record PatchTaskAdditionalInfoCommand(
    Guid TaskId,
    string? Priority,
    bool ClearPriority,
    DateOnly? EstimatedCompletionDate,
    bool ClearEstimatedCompletionDate,
    IReadOnlyList<string>? AddTags,
    IReadOnlyList<string>? RemoveTags,
    IReadOnlyDictionary<string, string>? SetMetadata,
    IReadOnlyList<string>? RemoveMetadata);
