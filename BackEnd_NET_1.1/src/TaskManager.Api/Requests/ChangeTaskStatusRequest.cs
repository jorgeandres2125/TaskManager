using TaskManager.Application.UseCases.Commands.Tasks;

namespace TaskManager.Api.Requests;

public record ChangeTaskStatusRequest(string? Status)
{
    public ChangeTaskStatusCommand ToCommand(Guid taskId) => new(taskId, Status);
}
