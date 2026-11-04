using TaskManager.Application.UseCases.Commands.Users;

namespace TaskManager.Api.Requests;

public record UpdateUserRequest(string? Name, string? Email)
{
    public UpdateUserCommand ToCommand(Guid userId) => new(userId, Name, Email);
}
