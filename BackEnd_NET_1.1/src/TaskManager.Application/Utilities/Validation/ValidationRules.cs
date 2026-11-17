using FluentValidation;
using TaskManager.Application.Utilities.Enums;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.Utilities.Validation;

/// <summary>Reglas de FluentValidation reutilizadas por varios validadores.</summary>
internal static class ValidationRules
{
    public static IRuleBuilderOptions<T, string?> MustBeEnumName<T, TEnum>(this IRuleBuilder<T, string?> rule)
        where TEnum : struct, Enum =>
        rule.Must(EnumNames.IsDefined<TEnum>)
            .WithMessage($"'{{PropertyName}}' debe ser uno de: {EnumNames.Describe<TEnum>()}.");

    public static IRuleBuilderOptions<T, string?> RequiredText<T>(this IRuleBuilder<T, string?> rule, int maxLength) =>
        rule.Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("'{PropertyName}' es obligatorio.")
            .MaximumLength(maxLength)
            .WithMessage($"'{{PropertyName}}' no puede superar {maxLength} caracteres.");

    public static IRuleBuilderOptions<T, string?> RequiredEmail<T>(this IRuleBuilder<T, string?> rule, int maxLength) =>
        rule.RequiredText(maxLength)
            .EmailAddress()
            .WithMessage("'{PropertyName}' no tiene un formato de correo válido.");

    public static IRuleBuilderOptions<T, Guid?> MustBeExistingUser<T>(this IRuleBuilderInitial<T, Guid?> rule, IUserRepository users) =>
        rule.Cascade(CascadeMode.Stop)
            .Must(userId => userId is not null && userId != Guid.Empty)
            .WithMessage("La tarea debe tener un usuario asignado.")
            .MustAsync((userId, cancellationToken) => users.ExistsAsync(userId!.Value, cancellationToken))
            .WithMessage("El usuario asignado no existe.");
}
