using TaskManager.Application.UseCases.Queries.Users;

namespace TaskManager.Api.Requests;

/// <summary>Búsqueda y paginación de <c>GET /api/users</c>, recibidas por query string.</summary>
public record GetUsersRequest
{
    public string? Search { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }

    public GetUsersQuery ToQuery() => new(Search, Page, PageSize);
}
