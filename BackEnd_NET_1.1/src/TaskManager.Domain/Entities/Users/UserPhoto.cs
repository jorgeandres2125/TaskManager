using TaskManager.Domain.Entities.Base;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Utilities.Validation;

namespace TaskManager.Domain.Entities.Users;

/// <summary>
/// Foto de un usuario, guardada en la base de datos como binario (VARBINARY(MAX)) junto con el nombre
/// del archivo original y su extensión. Va en una tabla aparte para que leer usuarios no cargue los bytes.
/// Un usuario tiene como máximo una foto; subir otra la reemplaza.
/// </summary>
public class UserPhoto : Entity
{
    public const int MaxSizeBytes = 2 * 1024 * 1024;
    public const int FileNameMaxLength = 255;
    public const int ExtensionMaxLength = 10;
    public const int ContentTypeMaxLength = 50;

    public Guid UserId { get; private set; }

    public string FileName { get; private set; } = null!;

    public string Extension { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;

    public int SizeBytes { get; private set; }

    public byte[] Content { get; private set; } = [];

    public DateTime? UpdatedAt { get; private set; }

    public DateTime Version => UpdatedAt ?? CreatedAt;

    private UserPhoto()
    {
    }

    private UserPhoto(Guid id, Guid userId, DateTime createdAt)
        : base(id, createdAt)
    {
        UserId = userId;
    }

    public static UserPhoto Create(
        Guid userId,
        string fileName,
        string extension,
        string contentType,
        byte[] content,
        DateTime createdAt)
    {
        var photo = new UserPhoto(NewId(), Guard.RequiredId(userId, nameof(UserId)), createdAt);
        photo.SetFile(fileName, extension, contentType, content);
        return photo;
    }

    public void Replace(string fileName, string extension, string contentType, byte[] content, DateTime updatedAt)
    {
        SetFile(fileName, extension, contentType, content);
        UpdatedAt = updatedAt;
    }

    private void SetFile(string fileName, string extension, string contentType, byte[] content)
    {
        if (content is null || content.Length == 0)
        {
            throw new DomainValidationException(nameof(Content), "La foto no puede estar vacía.");
        }

        if (content.Length > MaxSizeBytes)
        {
            throw new DomainValidationException(nameof(Content), $"La foto no puede superar {MaxSizeBytes / (1024 * 1024)} MB.");
        }

        FileName = Guard.RequiredText(fileName, nameof(FileName), FileNameMaxLength);
        Extension = Guard.RequiredText(extension, nameof(Extension), ExtensionMaxLength).ToLowerInvariant();
        ContentType = Guard.RequiredText(contentType, nameof(ContentType), ContentTypeMaxLength);
        Content = content;
        SizeBytes = content.Length;
    }
}
