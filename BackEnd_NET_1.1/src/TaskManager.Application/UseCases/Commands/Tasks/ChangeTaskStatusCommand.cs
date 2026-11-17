namespace TaskManager.Application.UseCases.Commands.Tasks;

public record ChangeTaskStatusCommand(Guid TaskId, string? Status);
