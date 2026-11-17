namespace TaskManager.Domain.Exceptions;

/// <summary>Un dato no cumple una invariante de una entidad.</summary>
public class DomainValidationException(string propertyName, string message) : DomainException(message)
{
    public string PropertyName { get; } = propertyName;
}
