namespace TaskManager.Application.DTOs.Tasks;

/// <param name="Number">Número correlativo de la tarea, para mostrarla como "#24".</param>
/// <param name="AllowedTransitions">
/// Estados a los que puede pasar la tarea desde su estado actual. Permite que el cliente no duplique la regla.
/// </param>
public record TaskDto(
    Guid Id,
    long Number,
    string Title,
    string Status,
    IReadOnlyList<string> AllowedTransitions,
    Guid UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    TaskAdditionalInfoDto? AdditionalInfo);
