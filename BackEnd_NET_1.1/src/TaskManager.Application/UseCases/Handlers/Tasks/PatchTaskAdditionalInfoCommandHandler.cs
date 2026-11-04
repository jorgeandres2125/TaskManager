using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Enums.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Domain.Entities.Tasks;

namespace TaskManager.Application.UseCases.Handlers.Tasks;

internal class PatchTaskAdditionalInfoCommandHandler(
    ITaskAdditionalInfoWriter writer,
    ITaskReadRepository taskReader,
    TimeProvider timeProvider) : IRequestHandler<PatchTaskAdditionalInfoCommand, TaskDto>
{
    public async Task<TaskDto> HandleAsync(PatchTaskAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        var result = await writer.PatchAsync(request.ToPatch(timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

        return result switch
        {
            TaskAdditionalInfoPatchResultEnum.Updated => await taskReader.GetRequiredAsync(request.TaskId, cancellationToken),
            TaskAdditionalInfoPatchResultEnum.TaskNotFound => throw new NotFoundException("la tarea", request.TaskId),
            TaskAdditionalInfoPatchResultEnum.TooManyTags => throw Invalid(
                nameof(PatchTaskAdditionalInfoCommand.AddTags),
                $"La tarea quedaría con más de {TaskAdditionalInfo.MaxTags} etiquetas."),
            TaskAdditionalInfoPatchResultEnum.TooManyMetadataEntries => throw Invalid(
                nameof(PatchTaskAdditionalInfoCommand.SetMetadata),
                $"La tarea quedaría con más de {TaskAdditionalInfo.MaxMetadataEntries} metadatos."),
            _ => throw new InvalidOperationException($"Resultado no previsto al modificar la tarea: {result}."),
        };
    }

    private static RequestValidationException Invalid(string propertyName, string message) =>
        new(new Dictionary<string, string[]> { [propertyName] = [message] });
}
