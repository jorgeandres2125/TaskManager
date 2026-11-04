namespace TaskManager.Tests.TestDoubles;

/// <summary>Contenidos mínimos con la firma de cada formato (no hace falta una imagen completa para las pruebas).</summary>
internal static class TestImages
{
    public static byte[] Jpeg => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    public static byte[] Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    public static byte[] Webp => [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBP"u8, .. "VP8 "u8];

    public static byte[] Pdf => [.. "%PDF-1.7"u8];
}
