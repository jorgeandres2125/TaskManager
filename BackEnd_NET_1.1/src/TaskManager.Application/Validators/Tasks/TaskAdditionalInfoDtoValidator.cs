using FluentValidation;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Validators.Tasks;

internal class TaskAdditionalInfoDtoValidator : AbstractValidator<TaskAdditionalInfoDto>
{
    public TaskAdditionalInfoDtoValidator()
    {
        RuleFor(info => info.Priority)
            .MustBeEnumName<TaskAdditionalInfoDto, TaskPriorityEnum>()
            .When(info => info.Priority is not null);

        RuleFor(info => info.Tags!.Count)
            .LessThanOrEqualTo(TaskAdditionalInfo.MaxTags)
            .WithName(nameof(TaskAdditionalInfoDto.Tags))
            .WithMessage($"Se admiten como máximo {TaskAdditionalInfo.MaxTags} etiquetas.")
            .When(info => info.Tags is not null);

        RuleForEach(info => info.Tags)
            .MaximumLength(TaskAdditionalInfo.TagMaxLength)
            .WithMessage($"Cada etiqueta admite como máximo {TaskAdditionalInfo.TagMaxLength} caracteres.");

        RuleFor(info => info.Metadata!.Count)
            .LessThanOrEqualTo(TaskAdditionalInfo.MaxMetadataEntries)
            .WithName(nameof(TaskAdditionalInfoDto.Metadata))
            .WithMessage($"Se admiten como máximo {TaskAdditionalInfo.MaxMetadataEntries} metadatos.")
            .When(info => info.Metadata is not null);

        RuleForEach(info => info.Metadata)
            .Must(entry => entry.Key.Length <= TaskAdditionalInfo.MetadataKeyMaxLength)
            .WithMessage($"Cada clave de metadatos admite como máximo {TaskAdditionalInfo.MetadataKeyMaxLength} caracteres.")
            .Must(entry => entry.Value is not null && entry.Value.Length <= TaskAdditionalInfo.MetadataValueMaxLength)
            .WithMessage($"Cada valor de metadatos es obligatorio y admite como máximo {TaskAdditionalInfo.MetadataValueMaxLength} caracteres.");
    }
}
