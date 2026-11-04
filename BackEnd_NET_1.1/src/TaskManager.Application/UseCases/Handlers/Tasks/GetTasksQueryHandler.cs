using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Enums.Sorting;
using TaskManager.Application.Mappings.Tasks;
using TaskManager.Application.Models.Pagination;
using TaskManager.Application.Models.Tasks;
using TaskManager.Application.UseCases.Queries.Tasks;
using TaskManager.Application.Utilities.Enums;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.UseCases.Handlers.Tasks;

internal class GetTasksQueryHandler(ITaskReadRepository taskReader)
    : IRequestHandler<GetTasksQuery, PagedResultDto<TaskDto>>
{
    public async Task<PagedResultDto<TaskDto>> HandleAsync(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var page = PageRequest.From(request);
        var criteria = new TaskSearchCriteria(
            UserId: request.UserId,
            Status: EnumNames.ParseOptional<TaskItemStatusEnum>(request.Status),
            Priority: EnumNames.ParseOptional<TaskPriorityEnum>(request.Priority),
            Tag: string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag.Trim(),
            SortDirection: EnumNames.ParseOptional<SortDirectionEnum>(request.SortDirection) ?? SortDirectionEnum.Desc,
            Page: page);

        var result = await taskReader.SearchAsync(criteria, cancellationToken);
        return PagedResultDto<TaskDto>.Create(result.Items.Select(task => task.ToDto()).ToArray(), page, result.TotalCount);
    }
}
