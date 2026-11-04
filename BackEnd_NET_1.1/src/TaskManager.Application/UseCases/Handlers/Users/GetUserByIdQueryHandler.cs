using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class GetUserByIdQueryHandler(IUserRepository users, IUserPhotoRepository photos)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("el usuario", request.Id);

        return await user.ToDtoAsync(photos, cancellationToken);
    }
}
