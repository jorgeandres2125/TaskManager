using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.UseCases.Commands.Tasks;

/// <summary>Reemplaza los datos editables de la tarea (PUT). El estado se cambia con <see cref="ChangeTaskStatusCommand"/>.</summary>
public record UpdateTaskCommand(Guid TaskId, string? Title, Guid? UserId, TaskAdditionalInfoDto? AdditionalInfo);
