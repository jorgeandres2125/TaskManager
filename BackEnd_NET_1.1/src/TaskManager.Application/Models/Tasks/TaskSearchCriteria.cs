using TaskManager.Application.Enums.Sorting;
using TaskManager.Application.Models.Pagination;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Models.Tasks;

/// <summary>
/// Todos los criterios son opcionales; el orden es siempre por fecha de creación.
/// Sin <paramref name="Page"/> se devuelven todos los resultados (por ejemplo, al buscar por Id).
/// </summary>
public record TaskSearchCriteria(
    Guid? TaskId = null,
    Guid? UserId = null,
    TaskItemStatusEnum? Status = null,
    TaskPriorityEnum? Priority = null,
    string? Tag = null,
    SortDirectionEnum SortDirection = SortDirectionEnum.Desc,
    PageRequest? Page = null)
{
    public static TaskSearchCriteria ById(Guid taskId) => new(TaskId: taskId);
}
