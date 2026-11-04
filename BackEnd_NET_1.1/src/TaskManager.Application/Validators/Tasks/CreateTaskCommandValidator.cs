using FluentValidation;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.Validators.Tasks;

internal class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator(IUserRepository users)
    {
        RuleFor(command => command.Title).RequiredText(TaskItem.TitleMaxLength);

        RuleFor(command => command.UserId).MustBeExistingUser(users);

        RuleFor(command => command.AdditionalInfo!)
            .SetValidator(new TaskAdditionalInfoDtoValidator())
            .When(command => command.AdditionalInfo is not null);
    }
}
