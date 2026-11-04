using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class GetUserPhotoQueryHandler(IUserPhotoRepository photos) : IRequestHandler<GetUserPhotoQuery, UserPhotoFileDto>
{
    public async Task<UserPhotoFileDto> HandleAsync(GetUserPhotoQuery request, CancellationToken cancellationToken)
    {
        var photo = await photos.GetAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("una foto del usuario", request.UserId);

        return new UserPhotoFileDto(photo.FileName, photo.Extension, photo.ContentType, photo.Content, photo.Version);
    }
}
