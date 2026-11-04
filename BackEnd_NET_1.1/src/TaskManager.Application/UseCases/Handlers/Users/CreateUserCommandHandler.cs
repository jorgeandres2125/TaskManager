using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class CreateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> HandleAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(request.Name!, request.Email!, timeProvider.GetUtcNow().UtcDateTime);

        if (await users.EmailExistsAsync(user.Email, exceptUserId: null, cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario con el correo '{user.Email}'.");
        }

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
