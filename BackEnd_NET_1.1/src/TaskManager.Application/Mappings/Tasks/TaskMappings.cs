using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Models.Tasks;
using TaskManager.Application.Utilities.Enums;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Domain.Utilities.Tasks;

namespace TaskManager.Application.Mappings.Tasks;

internal static class TaskMappings
{
    public static TaskDto ToDto(this TaskReadModel task) => new(
        task.Id,
        task.Number,
        task.Title,
        task.Status.ToString(),
        TaskStatusTransitions.GetAllowedTargets(task.Status).Select(status => status.ToString()).ToArray(),
        task.UserId,
        task.UserName,
        task.CreatedAt,
        task.UpdatedAt,
        task.AdditionalInfo?.ToDto());

    public static TaskAdditionalInfoDto ToDto(this TaskAdditionalInfo info) =>
        new(info.Priority?.ToString(), info.EstimatedCompletionDate, info.Tags, info.Metadata);

    public static TaskAdditionalInfo? ToDomain(this TaskAdditionalInfoDto? dto) =>
        dto is null
            ? null
            : TaskAdditionalInfo.Create(
                EnumNames.ParseOptional<TaskPriorityEnum>(dto.Priority),
                dto.EstimatedCompletionDate,
                dto.Tags,
                dto.Metadata);

    public static async Task<TaskDto> GetRequiredAsync(
        this ITaskReadRepository reader,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var result = await reader.SearchAsync(TaskSearchCriteria.ById(taskId), cancellationToken);
        return result.Items.SingleOrDefault()?.ToDto() ?? throw new NotFoundException("la tarea", taskId);
    }
}
