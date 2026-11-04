namespace TaskManager.Application.DTOs.Tasks;

public record TaskAdditionalInfoDto(
    string? Priority,
    DateOnly? EstimatedCompletionDate,
    IReadOnlyList<string>? Tags,
    IReadOnlyDictionary<string, string>? Metadata);
