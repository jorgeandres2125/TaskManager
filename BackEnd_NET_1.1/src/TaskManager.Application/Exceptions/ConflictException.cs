namespace TaskManager.Application.Exceptions;

/// <summary>La petición entra en conflicto con el estado actual de los datos (por ejemplo, un correo duplicado).</summary>
public class ConflictException(string message) : Exception(message);
