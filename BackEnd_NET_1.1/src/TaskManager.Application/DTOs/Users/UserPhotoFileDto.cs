namespace TaskManager.Application.DTOs.Users;

/// <summary>Foto lista para enviarse como archivo: nombre original, tipo MIME, bytes y versión (para la caché).</summary>
public record UserPhotoFileDto(string FileName, string Extension, string ContentType, byte[] Content, DateTime Version);
