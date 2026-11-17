using System.Text.RegularExpressions;
using FluentValidation;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Validators.Tasks;

/// <summary>
/// Mismos límites que la información adicional completa. Además, las claves de metadatos solo admiten
/// letras, números, guion y guion bajo, porque forman parte de la ruta JSON ($.metadata."clave") de JSON_MODIFY.
/// </summary>
internal partial class PatchTaskAdditionalInfoCommandValidator : AbstractValidator<PatchTaskAdditionalInfoCommand>
{
    private static readonly string MetadataKeyMessage =
        $"Cada clave de metadatos admite como máximo {TaskAdditionalInfo.MetadataKeyMaxLength} caracteres: letras sin tilde, números, '-' y '_'.";

    public PatchTaskAdditionalInfoCommandValidator()
    {
        RuleFor(command => command)
            .Must(HasChanges)
            .OverridePropertyName("AdditionalInfo")
            .WithMessage("Indique al menos un cambio en la información adicional.");

        RuleFor(command => command.Priority)
            .MustBeEnumName<PatchTaskAdditionalInfoCommand, TaskPriorityEnum>()
            .When(command => command.Priority is not null);

        RuleFor(command => command.ClearPriority)
            .Equal(false)
            .When(command => command.Priority is not null)
            .WithMessage("No se puede asignar y quitar la prioridad a la vez.");

        RuleFor(command => command.ClearEstimatedCompletionDate)
            .Equal(false)
            .When(command => command.EstimatedCompletionDate is not null)
            .WithMessage("No se puede asignar y quitar la fecha estimada a la vez.");

        RuleFor(command => command.AddTags!.Count)
            .LessThanOrEqualTo(TaskAdditionalInfo.MaxTags)
            .OverridePropertyName(nameof(PatchTaskAdditionalInfoCommand.AddTags))
            .WithMessage($"Se admiten como máximo {TaskAdditionalInfo.MaxTags} etiquetas.")
            .When(command => command.AddTags is not null);

        RuleForEach(command => command.AddTags)
            .MaximumLength(TaskAdditionalInfo.TagMaxLength)
            .WithMessage($"Cada etiqueta admite como máximo {TaskAdditionalInfo.TagMaxLength} caracteres.")
            .Must((command, tag) => tag is null || !(command.RemoveTags ?? []).Contains(tag.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Una etiqueta no puede añadirse y quitarse a la vez.");

        RuleForEach(command => command.RemoveTags)
            .MaximumLength(TaskAdditionalInfo.TagMaxLength)
            .WithMessage($"Cada etiqueta admite como máximo {TaskAdditionalInfo.TagMaxLength} caracteres.");

        RuleFor(command => command.SetMetadata!.Count)
            .LessThanOrEqualTo(TaskAdditionalInfo.MaxMetadataEntries)
            .OverridePropertyName(nameof(PatchTaskAdditionalInfoCommand.SetMetadata))
            .WithMessage($"Se admiten como máximo {TaskAdditionalInfo.MaxMetadataEntries} metadatos.")
            .When(command => command.SetMetadata is not null);

        RuleForEach(command => command.SetMetadata)
            .Must(entry => IsValidMetadataKey(entry.Key))
            .WithMessage(MetadataKeyMessage)
            .Must(entry => entry.Value is not null && entry.Value.Length <= TaskAdditionalInfo.MetadataValueMaxLength)
            .WithMessage($"Cada valor de metadatos es obligatorio y admite como máximo {TaskAdditionalInfo.MetadataValueMaxLength} caracteres.")
            .Must((command, entry) => !(command.RemoveMetadata ?? []).Contains(entry.Key))
            .WithMessage("Un metadato no puede asignarse y quitarse a la vez.");

        RuleForEach(command => command.RemoveMetadata)
            .Must(IsValidMetadataKey)
            .WithMessage(MetadataKeyMessage);
    }

    private static bool HasChanges(PatchTaskAdditionalInfoCommand command) =>
        command.Priority is not null
        || command.ClearPriority
        || command.EstimatedCompletionDate is not null
        || command.ClearEstimatedCompletionDate
        || command.AddTags is { Count: > 0 }
        || command.RemoveTags is { Count: > 0 }
        || command.SetMetadata is { Count: > 0 }
        || command.RemoveMetadata is { Count: > 0 };

    private static bool IsValidMetadataKey(string? key) =>
        key is not null && key.Length <= TaskAdditionalInfo.MetadataKeyMaxLength && MetadataKeyPattern().IsMatch(key);

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex MetadataKeyPattern();
}
