using TaskManager.Application.UseCases.Queries.Tasks;

namespace TaskManager.Api.Requests;

/// <summary>Filtros y paginación opcionales de <c>GET /api/tasks</c>, recibidos por query string.</summary>
public record GetTasksRequest
{
    public Guid? UserId { get; init; }

    public string? Status { get; init; }

    public string? Priority { get; init; }

    public string? Tag { get; init; }

    public string? SortDirection { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }

    public GetTasksQuery ToQuery() => new(UserId, Status, Priority, Tag, SortDirection, Page, PageSize);
}
