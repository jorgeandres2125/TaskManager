namespace TaskManager.Application.Exceptions;

public class NotFoundException(string resourceName, Guid id)
    : Exception($"No existe {resourceName} con Id '{id}'.");
