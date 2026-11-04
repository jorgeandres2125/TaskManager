using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;

namespace TaskManager.Api.Requests;

public record CreateTaskRequest(string? Title, Guid? UserId, TaskAdditionalInfoDto? AdditionalInfo)
{
    public CreateTaskCommand ToCommand() => new(Title, UserId, AdditionalInfo);
}
