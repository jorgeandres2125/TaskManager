using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.Models.Pagination;
using TaskManager.Application.UseCases.Handlers.Users;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Application.Validators.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class PaginationTests
{
    [Fact]
    public void PageRequest_WithoutValues_UsesDefaults()
    {
        var page = PageRequest.From(new GetUsersQuery(null, null, null));

        Assert.Equal(PageRequest.FirstPage, page.Page);
        Assert.Equal(PageRequest.DefaultPageSize, page.PageSize);
        Assert.Equal(0, page.Skip);
    }

    [Theory]
    [InlineData(1, 20, 45, 3, false, true)]
    [InlineData(3, 20, 45, 3, true, false)]
    [InlineData(1, 20, 0, 0, false, false)]
    [InlineData(5, 20, 45, 3, true, false)]
    public void PagedResult_ComputesNavigation(int page, int pageSize, int total, int totalPages, bool hasPrevious, bool hasNext)
    {
        var result = new PagedResultDto<string>([], page, pageSize, total);

        Assert.Equal(totalPages, result.TotalPages);
        Assert.Equal(hasPrevious, result.HasPreviousPage);
        Assert.Equal(hasNext, result.HasNextPage);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(1, 1, true)]
    [InlineData(10, PageRequest.MaxPageSize, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, PageRequest.MaxPageSize + 1, false)]
    public void Validator_ChecksPageLimits(int? page, int? pageSize, bool expected)
    {
        Assert.Equal(expected, new GetUsersQueryValidator().Validate(new GetUsersQuery(null, page, pageSize)).IsValid);
    }

    [Fact]
    public async Task GetUsers_ReturnsRequestedPageOrderedByName()
    {
        var users = new InMemoryUserRepository();
        foreach (var name in new[] { "Eva", "Ana", "Dora", "Carla", "Bea" })
        {
            users.Add(User.Create(name, $"{name}@acme.com", FixedTimeProvider.DefaultUtcNow));
        }

        var result = await new GetUsersQueryHandler(users, new InMemoryUserPhotoRepository()).HandleAsync(new GetUsersQuery(null, 2, 2), CancellationToken.None);

        Assert.Equal(["Carla", "Dora"], result.Items.Select(user => user.Name));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetUsers_WithSearch_FiltersByNameOrEmailAndCountsMatches()
    {
        var users = new InMemoryUserRepository();
        users.Add(User.Create("Ana Gómez", "ana@acme.com", FixedTimeProvider.DefaultUtcNow));
        users.Add(User.Create("Mariana Ruiz", "mruiz@acme.com", FixedTimeProvider.DefaultUtcNow));
        users.Add(User.Create("Carlos Pérez", "carlos.ana@acme.com", FixedTimeProvider.DefaultUtcNow));
        users.Add(User.Create("Laura Díaz", "laura@acme.com", FixedTimeProvider.DefaultUtcNow));

        var result = await new GetUsersQueryHandler(users, new InMemoryUserPhotoRepository()).HandleAsync(new GetUsersQuery("  ANA ", 1, 20), CancellationToken.None);

        Assert.Equal(["Ana Gómez", "Carlos Pérez", "Mariana Ruiz"], result.Items.Select(user => user.Name));
        Assert.Equal(3, result.TotalCount);
    }
}
