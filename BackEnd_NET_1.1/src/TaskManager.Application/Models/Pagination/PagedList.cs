namespace TaskManager.Application.Models.Pagination;

/// <summary>Una página de resultados junto con el total de elementos que cumplen los filtros.</summary>
public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount);
