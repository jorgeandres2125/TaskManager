namespace TaskManager.Application.UseCases.Commands.Users;

/// <summary>Quita la foto del usuario. Si no tiene, no es un error (DELETE idempotente).</summary>
public record DeleteUserPhotoCommand(Guid UserId);
