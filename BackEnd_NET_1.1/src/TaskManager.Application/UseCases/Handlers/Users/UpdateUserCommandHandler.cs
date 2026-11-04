using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class UpdateUserCommandHandler(
    IUserRepository users,
    IUserPhotoRepository photos,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> HandleAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetForUpdateAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("el usuario", request.UserId);

        var email = User.NormalizeEmail(request.Email);
        if (await users.EmailExistsAsync(email, exceptUserId: user.Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario con el correo '{email}'.");
        }

        user.Update(request.Name!, email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await user.ToDtoAsync(photos, cancellationToken);
    }
}
