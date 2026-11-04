using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.Utilities.Images;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class UploadUserPhotoCommandHandler(
    IUserRepository users,
    IUserPhotoRepository photos,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UploadUserPhotoCommand, UserDto>
{
    public async Task<UserDto> HandleAsync(UploadUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("el usuario", request.UserId);

        var fileName = Path.GetFileName(request.FileName!.Trim());
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = ImageFormats.ContentTypeOf(ImageFormats.Detect(request.Content)!.Value);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var photo = await photos.GetForUpdateAsync(user.Id, cancellationToken);
        if (photo is null)
        {
            photo = UserPhoto.Create(user.Id, fileName, extension, contentType, request.Content!, now);
            photos.Add(photo);
        }
        else
        {
            photo.Replace(fileName, extension, contentType, request.Content!, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto(photo.Version);
    }
}
