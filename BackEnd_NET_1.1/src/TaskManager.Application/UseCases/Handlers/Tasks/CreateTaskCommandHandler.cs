using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Mappings.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Interfaces.Persistence;
using TaskManager.Domain.Interfaces.Tasks;

namespace TaskManager.Application.UseCases.Handlers.Tasks;

internal class CreateTaskCommandHandler(
    ITaskRepository tasks,
    ITaskReadRepository taskReader,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreateTaskCommand, TaskDto>
{
    public async Task<TaskDto> HandleAsync(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = TaskItem.Create(
            request.Title!,
            request.UserId.GetValueOrDefault(),
            request.AdditionalInfo.ToDomain(),
            timeProvider.GetUtcNow().UtcDateTime);

        tasks.Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await taskReader.GetRequiredAsync(task.Id, cancellationToken);
    }
}
