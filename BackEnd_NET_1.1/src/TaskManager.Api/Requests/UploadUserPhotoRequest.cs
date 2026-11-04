using TaskManager.Application.UseCases.Commands.Users;

namespace TaskManager.Api.Requests;

/// <summary>Formulario multipart con el archivo de la foto (campo "file").</summary>
public record UploadUserPhotoRequest(IFormFile? File)
{
    public async Task<UploadUserPhotoCommand> ToCommandAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (File is null)
        {
            return new UploadUserPhotoCommand(userId, null, null);
        }

        using var buffer = new MemoryStream();
        await File.CopyToAsync(buffer, cancellationToken);
        return new UploadUserPhotoCommand(userId, File.FileName, buffer.ToArray());
    }
}
