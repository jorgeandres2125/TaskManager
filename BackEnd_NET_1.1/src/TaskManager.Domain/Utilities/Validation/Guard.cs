using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Utilities.Validation;

/// <summary>Validaciones de invariantes reutilizadas por las entidades.</summary>
internal static class Guard
{
    public static string RequiredText(string? value, string propertyName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(propertyName, $"{propertyName} es obligatorio.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException(propertyName, $"{propertyName} no puede superar {maxLength} caracteres.");
        }

        return trimmed;
    }

    public static Guid RequiredId(Guid value, string propertyName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainValidationException(propertyName, $"{propertyName} es obligatorio.");
        }

        return value;
    }
}
