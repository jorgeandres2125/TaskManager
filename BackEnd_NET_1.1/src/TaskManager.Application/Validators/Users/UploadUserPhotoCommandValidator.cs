using FluentValidation;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.Utilities.Images;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Entities.Users;

namespace TaskManager.Application.Validators.Users;

/// <summary>
/// La foto debe ser JPEG, PNG o WebP de hasta 2 MB, y su extensión debe coincidir con su contenido real
/// (por ejemplo, un PDF renombrado a .jpg se rechaza).
/// </summary>
internal class UploadUserPhotoCommandValidator : AbstractValidator<UploadUserPhotoCommand>
{
    public UploadUserPhotoCommandValidator()
    {
        RuleFor(command => command.FileName)
            .Cascade(CascadeMode.Stop)
            .RequiredText(UserPhoto.FileNameMaxLength)
            .Must(fileName => ImageFormats.FromExtension(Path.GetExtension(fileName)) is not null)
            .WithMessage($"La foto debe tener una de estas extensiones: {ImageFormats.AllowedExtensions}.");

        RuleFor(command => command.Content)
            .Cascade(CascadeMode.Stop)
            .Must(content => content is { Length: > 0 })
            .WithMessage("Seleccione una foto.")
            .Must(content => content!.Length <= UserPhoto.MaxSizeBytes)
            .WithMessage($"La foto no puede superar {UserPhoto.MaxSizeBytes / (1024 * 1024)} MB.")
            .Must(content => ImageFormats.Detect(content) is not null)
            .WithMessage("El archivo no es una imagen JPEG, PNG o WebP válida.")
            .Must((command, content) =>
                ImageFormats.FromExtension(Path.GetExtension(command.FileName)) is not { } declared ||
                ImageFormats.Detect(content) == declared)
            .WithMessage("La extensión del archivo no coincide con su contenido.");
    }
}
