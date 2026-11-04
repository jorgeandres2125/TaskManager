using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Models.Tasks;

/// <summary>Proyección de lectura de una tarea, con el nombre del usuario asignado.</summary>
public record TaskReadModel(
    Guid Id,
    long Number,
    string Title,
    TaskItemStatusEnum Status,
    Guid UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    TaskAdditionalInfo? AdditionalInfo);
