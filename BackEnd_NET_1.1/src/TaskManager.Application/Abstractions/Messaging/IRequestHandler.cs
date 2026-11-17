namespace TaskManager.Application.Abstractions.Messaging;

/// <summary>
/// Caso de uso de la aplicación: un comando (modifica estado) o una consulta (solo lectura).
/// Cada caso de uso tiene su propio handler, de modo que los controllers solo dependen de lo que usan.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
