using TaskManager.Application.Models.Pagination;
using TaskManager.Application.Models.Tasks;
using TaskManager.Domain.Interfaces.Tasks;

namespace TaskManager.Application.Abstractions.Tasks;

/// <summary>
/// Lado de lectura de tareas (CQRS): proyecciones que combinan Tasks, Users y campos del JSON.
/// Las escrituras usan <see cref="ITaskRepository"/> del dominio.
/// </summary>
public interface ITaskReadRepository
{
    Task<PagedList<TaskReadModel>> SearchAsync(TaskSearchCriteria criteria, CancellationToken cancellationToken);
}
