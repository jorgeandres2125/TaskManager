using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Middlewares;

/// <summary>
/// Traduce las excepciones de la aplicación y del dominio a respuestas ProblemDetails (RFC 9457)
/// con un formato uniforme. Los errores no previstos se registran y se devuelven sin detalles internos.
/// </summary>
internal class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var problem = exception switch
        {
            RequestValidationException validation => ValidationProblem(validation.Errors),
            DomainValidationException domainValidation => ValidationProblem(
                new Dictionary<string, string[]> { [domainValidation.PropertyName] = [domainValidation.Message] }),
            NotFoundException notFound => Problem(StatusCodes.Status404NotFound, "Recurso no encontrado", notFound.Message),
            InvalidTaskStatusTransitionException transition =>
                Problem(StatusCodes.Status409Conflict, "Transición de estado no permitida", transition.Message),
            ConflictException conflict => Problem(StatusCodes.Status409Conflict, "Conflicto", conflict.Message),
            _ => null,
        };

        if (problem is null)
        {
            logger.LogError(exception, "Error no controlado al procesar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            problem = Problem(
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado. Intente nuevamente más tarde.");
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ValidationProblemDetails ValidationProblem(IReadOnlyDictionary<string, string[]> errors) =>
        new(errors.ToDictionary(error => ToJsonPath(error.Key), error => error.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Uno o más datos no son válidos.",
        };

    private static string ToJsonPath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
