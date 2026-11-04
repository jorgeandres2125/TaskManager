using FluentValidation;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Application.Validators.Tasks;

internal class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(command => command.Status).MustBeEnumName<ChangeTaskStatusCommand, TaskItemStatusEnum>();
    }
}
