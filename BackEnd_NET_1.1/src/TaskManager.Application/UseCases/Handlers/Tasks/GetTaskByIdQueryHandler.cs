using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Mappings.Tasks;
using TaskManager.Application.UseCases.Queries.Tasks;

namespace TaskManager.Application.UseCases.Handlers.Tasks;

internal class GetTaskByIdQueryHandler(ITaskReadRepository taskReader) : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    public Task<TaskDto> HandleAsync(GetTaskByIdQuery request, CancellationToken cancellationToken) =>
        taskReader.GetRequiredAsync(request.Id, cancellationToken);
}
