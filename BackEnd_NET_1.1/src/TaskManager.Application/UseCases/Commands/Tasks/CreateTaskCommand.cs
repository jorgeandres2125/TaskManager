using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.UseCases.Commands.Tasks;

public record CreateTaskCommand(string? Title, Guid? UserId, TaskAdditionalInfoDto? AdditionalInfo);
