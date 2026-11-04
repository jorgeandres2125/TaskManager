using TaskManager.Domain.Enums.Users;

namespace TaskManager.Application.Utilities.Images;

/// <summary>
/// Reconoce el formato de una imagen por sus primeros bytes (firma), su tipo MIME y sus extensiones.
/// La firma evita aceptar un archivo con una extensión de imagen que no es una imagen.
/// </summary>
internal static class ImageFormats
{
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] RiffSignature = "RIFF"u8.ToArray();
    private static readonly byte[] WebpSignature = "WEBP"u8.ToArray();

    private static readonly IReadOnlyDictionary<string, UserPhotoFormatEnum> FormatsByExtension =
        new Dictionary<string, UserPhotoFormatEnum>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = UserPhotoFormatEnum.Jpeg,
            [".jpeg"] = UserPhotoFormatEnum.Jpeg,
            [".png"] = UserPhotoFormatEnum.Png,
            [".webp"] = UserPhotoFormatEnum.Webp,
        };

    public static string AllowedExtensions => string.Join(", ", FormatsByExtension.Keys);

    public static UserPhotoFormatEnum? FromExtension(string? extension) =>
        extension is not null && FormatsByExtension.TryGetValue(extension, out var format) ? format : null;

    public static UserPhotoFormatEnum? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(JpegSignature))
        {
            return UserPhotoFormatEnum.Jpeg;
        }

        if (content.StartsWith(PngSignature))
        {
            return UserPhotoFormatEnum.Png;
        }

        if (content.Length >= 12 && content.StartsWith(RiffSignature) && content[8..12].SequenceEqual(WebpSignature))
        {
            return UserPhotoFormatEnum.Webp;
        }

        return null;
    }

    public static string ContentTypeOf(UserPhotoFormatEnum format) => format switch
    {
        UserPhotoFormatEnum.Jpeg => "image/jpeg",
        UserPhotoFormatEnum.Png => "image/png",
        UserPhotoFormatEnum.Webp => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };
}
