using FluentValidation;
using TaskManager.Application.Enums.Sorting;
using TaskManager.Application.UseCases.Queries.Tasks;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Application.Validators.Pagination;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Validators.Tasks;

internal class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(query => query.Status)
            .MustBeEnumName<GetTasksQuery, TaskItemStatusEnum>()
            .When(query => query.Status is not null);

        RuleFor(query => query.Priority)
            .MustBeEnumName<GetTasksQuery, TaskPriorityEnum>()
            .When(query => query.Priority is not null);

        RuleFor(query => query.Tag)
            .MaximumLength(TaskAdditionalInfo.TagMaxLength)
            .WithMessage($"'{{PropertyName}}' no puede superar {TaskAdditionalInfo.TagMaxLength} caracteres.");

        RuleFor(query => query.SortDirection)
            .MustBeEnumName<GetTasksQuery, SortDirectionEnum>()
            .When(query => query.SortDirection is not null);
    }
}
