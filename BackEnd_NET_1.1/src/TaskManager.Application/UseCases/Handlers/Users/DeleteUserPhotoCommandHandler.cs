using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class DeleteUserPhotoCommandHandler(
    IUserRepository users,
    IUserPhotoRepository photos,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserPhotoCommand, UserDto>
{
    public async Task<UserDto> HandleAsync(DeleteUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("el usuario", request.UserId);

        var photo = await photos.GetForUpdateAsync(user.Id, cancellationToken);
        if (photo is not null)
        {
            photos.Remove(photo);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return user.ToDto();
    }
}
