using FluentValidation;
using TaskManager.Application.Abstractions.Pagination;
using TaskManager.Application.Models.Pagination;

namespace TaskManager.Application.Validators.Pagination;

/// <summary>Reglas de paginación comunes; los validadores de las consultas de listas las incluyen con <c>Include</c>.</summary>
internal class PagedRequestValidator : AbstractValidator<IPagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(PageRequest.FirstPage)
            .WithMessage($"'{{PropertyName}}' debe ser mayor o igual que {PageRequest.FirstPage}.")
            .When(request => request.Page is not null);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .WithMessage($"'{{PropertyName}}' debe estar entre 1 y {PageRequest.MaxPageSize}.")
            .When(request => request.PageSize is not null);
    }
}
