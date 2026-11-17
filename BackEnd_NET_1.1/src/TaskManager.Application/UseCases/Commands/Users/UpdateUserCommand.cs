namespace TaskManager.Application.UseCases.Commands.Users;

public record UpdateUserCommand(Guid UserId, string? Name, string? Email);
