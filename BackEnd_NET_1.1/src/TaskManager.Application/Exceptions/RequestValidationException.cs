namespace TaskManager.Application.Exceptions;

/// <summary>La petición no supera la validación de entrada. Los errores se agrupan por propiedad.</summary>
public class RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Uno o más datos de la petición no son válidos.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
