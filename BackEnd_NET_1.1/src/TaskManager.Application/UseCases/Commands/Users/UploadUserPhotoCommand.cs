namespace TaskManager.Application.UseCases.Commands.Users;

/// <summary>Sube o reemplaza la foto del usuario. <paramref name="FileName"/> es el nombre del archivo original.</summary>
public record UploadUserPhotoCommand(Guid UserId, string? FileName, byte[]? Content);
