using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;

namespace TaskManager.Api.Requests;

public record UpdateTaskRequest(string? Title, Guid? UserId, TaskAdditionalInfoDto? AdditionalInfo)
{
    public UpdateTaskCommand ToCommand(Guid taskId) => new(taskId, Title, UserId, AdditionalInfo);
}
