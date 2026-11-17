using TaskManager.Application.Abstractions.Pagination;

namespace TaskManager.Application.Models.Pagination;

/// <summary>Página solicitada, con los valores por defecto ya aplicados.</summary>
public record PageRequest(int Page, int PageSize)
{
    public const int FirstPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Skip => (Page - 1) * PageSize;

    public static PageRequest From(IPagedRequest request) =>
        new(request.Page ?? FirstPage, request.PageSize ?? DefaultPageSize);
}
