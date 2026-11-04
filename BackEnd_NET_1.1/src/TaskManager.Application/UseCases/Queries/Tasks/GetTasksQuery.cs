using TaskManager.Application.Abstractions.Pagination;
using TaskManager.Application.Enums.Sorting;

namespace TaskManager.Application.UseCases.Queries.Tasks;

/// <summary>Tareas filtradas, ordenadas por fecha de creación y paginadas.</summary>
/// <param name="SortDirection">
/// Nombre de un <see cref="SortDirectionEnum"/>: <c>desc</c> (por defecto, más recientes primero) o <c>asc</c>.
/// </param>
public record GetTasksQuery(
    Guid? UserId,
    string? Status,
    string? Priority,
    string? Tag,
    string? SortDirection,
    int? Page,
    int? PageSize) : IPagedRequest;
