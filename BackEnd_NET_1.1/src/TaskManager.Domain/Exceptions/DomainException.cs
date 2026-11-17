namespace TaskManager.Domain.Exceptions;

/// <summary>Violación de una regla de negocio del dominio.</summary>
public abstract class DomainException(string message) : Exception(message);
