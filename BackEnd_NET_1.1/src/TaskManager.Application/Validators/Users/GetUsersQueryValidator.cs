using FluentValidation;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Application.Validators.Pagination;

namespace TaskManager.Application.Validators.Users;

internal class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(query => query.Search)
            .MaximumLength(GetUsersQuery.SearchMaxLength)
            .WithMessage($"'{{PropertyName}}' no puede superar {GetUsersQuery.SearchMaxLength} caracteres.");
    }
}
