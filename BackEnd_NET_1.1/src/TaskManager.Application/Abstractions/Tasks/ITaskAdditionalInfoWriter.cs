using TaskManager.Application.Enums.Tasks;
using TaskManager.Application.Models.Tasks;
using TaskManager.Domain.Interfaces.Tasks;

namespace TaskManager.Application.Abstractions.Tasks;

/// <summary>
/// Modifica campos sueltos del JSON de una tarea directamente en SQL Server (JSON_MODIFY), sin leer ni
/// reescribir el documento completo como hace <see cref="ITaskRepository"/>.
/// </summary>
public interface ITaskAdditionalInfoWriter
{
    Task<TaskAdditionalInfoPatchResultEnum> PatchAsync(TaskAdditionalInfoPatch patch, CancellationToken cancellationToken);
}
