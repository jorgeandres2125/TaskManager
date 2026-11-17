namespace TaskManager.Application.DTOs.Users;

/// <param name="PhotoUpdatedAt">Versión de la foto del usuario; null si no tiene foto.</param>
public record UserDto(Guid Id, string Name, string Email, DateTime CreatedAt, DateTime? PhotoUpdatedAt = null);
