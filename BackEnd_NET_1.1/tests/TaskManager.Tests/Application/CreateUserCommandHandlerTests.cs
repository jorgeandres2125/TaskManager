using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.UseCases.Handlers.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class CreateUserCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateUserCommandHandler CreateHandler() => new(_users, _unitOfWork, new FixedTimeProvider());

    [Fact]
    public async Task HandleAsync_WithNewEmail_SavesNormalizedUser()
    {
        var result = await CreateHandler().HandleAsync(new CreateUserCommand("Ana", " ANA@Acme.com "), CancellationToken.None);

        Assert.Equal("ana@acme.com", result.Email);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, result.CreatedAt);
        Assert.Single(_users.Users);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ThrowsConflictWithoutSaving()
    {
        _users.Add(User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateHandler().HandleAsync(new CreateUserCommand("Otra Ana", "Ana@Acme.com"), CancellationToken.None));

        Assert.Single(_users.Users);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
