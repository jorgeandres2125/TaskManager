using TaskManager.Application.Abstractions.Pagination;

namespace TaskManager.Application.UseCases.Queries.Users;

/// <summary>Usuarios ordenados por nombre, paginados.</summary>
/// <param name="Search">Texto opcional que se busca en el nombre o el correo.</param>
public record GetUsersQuery(string? Search, int? Page, int? PageSize) : IPagedRequest
{
    public const int SearchMaxLength = 100;
}
