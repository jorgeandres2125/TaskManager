using FluentValidation;
using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Exceptions;

namespace TaskManager.Application.Behaviors;

/// <summary>
/// Decorador que ejecuta los validadores de una petición antes de su handler,
/// para que ningún caso de uso tenga que repetir esa lógica.
/// </summary>
internal class ValidationBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    IEnumerable<IValidator<TRequest>> validators) : IRequestHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

            throw new RequestValidationException(errors);
        }

        return await inner.HandleAsync(request, cancellationToken);
    }
}
