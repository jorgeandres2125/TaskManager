using TaskManager.Application.UseCases.Commands.Users;

namespace TaskManager.Api.Requests;

public record CreateUserRequest(string? Name, string? Email)
{
    public CreateUserCommand ToCommand() => new(Name, Email);
}
