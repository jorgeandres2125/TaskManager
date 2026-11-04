namespace TaskManager.Application.Abstractions.Pagination;

/// <summary>Consulta que devuelve una lista paginada. Ambos valores son opcionales y tienen valores por defecto.</summary>
public interface IPagedRequest
{
    int? Page { get; }

    int? PageSize { get; }
}
