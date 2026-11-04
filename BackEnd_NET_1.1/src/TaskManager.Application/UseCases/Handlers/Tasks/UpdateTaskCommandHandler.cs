using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Mappings.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Tasks;

namespace TaskManager.Application.UseCases.Handlers.Tasks;

internal class UpdateTaskCommandHandler(
    ITaskRepository tasks,
    ITaskReadRepository taskReader,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    public async Task<TaskDto> HandleAsync(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("la tarea", request.TaskId);

        task.Update(
            request.Title!,
            request.UserId.GetValueOrDefault(),
            request.AdditionalInfo.ToDomain(),
            timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await taskReader.GetRequiredAsync(task.Id, cancellationToken);
    }
}
