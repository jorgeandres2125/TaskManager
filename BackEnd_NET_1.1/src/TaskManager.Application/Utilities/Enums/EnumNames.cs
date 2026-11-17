namespace TaskManager.Application.Utilities.Enums;

/// <summary>
/// Los enums del dominio viajan como texto en la API. Aquí se centraliza cómo se describen y convierten,
/// sin aceptar valores numéricos.
/// </summary>
internal static class EnumNames
{
    public static string Describe<TEnum>() where TEnum : struct, Enum => string.Join(", ", Enum.GetNames<TEnum>());

    public static bool IsDefined<TEnum>(string? value) where TEnum : struct, Enum =>
        value is not null && Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);

    public static TEnum Parse<TEnum>(string value) where TEnum : struct, Enum => Enum.Parse<TEnum>(value, ignoreCase: true);

    public static TEnum? ParseOptional<TEnum>(string? value) where TEnum : struct, Enum =>
        string.IsNullOrWhiteSpace(value) ? null : Parse<TEnum>(value);
}
