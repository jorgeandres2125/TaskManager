using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Mappings.Users;
using TaskManager.Application.Models.Pagination;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.UseCases.Handlers.Users;

internal class GetUsersQueryHandler(IUserRepository users, IUserPhotoRepository photos)
    : IRequestHandler<GetUsersQuery, PagedResultDto<UserDto>>
{
    public async Task<PagedResultDto<UserDto>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = PageRequest.From(request);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        var totalCount = await users.CountAsync(search, cancellationToken);
        var pageItems = await users.ListAsync(search, page.Skip, page.PageSize, cancellationToken);

        var items = await pageItems.ToDtosAsync(photos, cancellationToken);

        return PagedResultDto<UserDto>.Create(items, page, totalCount);
    }
}
