using FluentValidation;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.Utilities.Validation;
using TaskManager.Domain.Entities.Users;

namespace TaskManager.Application.Validators.Users;

internal class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Name).RequiredText(User.NameMaxLength);

        RuleFor(command => command.Email).RequiredEmail(User.EmailMaxLength);
    }
}
