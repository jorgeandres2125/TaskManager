using TaskManager.Application.UseCases.Commands.Tasks;

namespace TaskManager.Api.Requests;

public record PatchTaskAdditionalInfoRequest(
    string? Priority,
    bool ClearPriority,
    DateOnly? EstimatedCompletionDate,
    bool ClearEstimatedCompletionDate,
    IReadOnlyList<string>? AddTags,
    IReadOnlyList<string>? RemoveTags,
    IReadOnlyDictionary<string, string>? SetMetadata,
    IReadOnlyList<string>? RemoveMetadata)
{
    public PatchTaskAdditionalInfoCommand ToCommand(Guid taskId) => new(
        taskId,
        Priority,
        ClearPriority,
        EstimatedCompletionDate,
        ClearEstimatedCompletionDate,
        AddTags,
        RemoveTags,
        SetMetadata,
        RemoveMetadata);
}
