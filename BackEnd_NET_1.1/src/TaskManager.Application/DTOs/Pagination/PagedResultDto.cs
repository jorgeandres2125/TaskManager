using TaskManager.Application.Models.Pagination;

namespace TaskManager.Application.DTOs.Pagination;

/// <summary>Respuesta de las consultas que devuelven listas.</summary>
public record PagedResultDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > PageRequest.FirstPage;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResultDto<T> Create(IReadOnlyList<T> items, PageRequest page, int totalCount) =>
        new(items, page.Page, page.PageSize, totalCount);
}
