using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.UseCases.Handlers.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class UpdateUserCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly User _ana = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);

    public UpdateUserCommandHandlerTests()
    {
        _users.Add(_ana);
        _users.Add(User.Create("Luis", "luis@acme.com", FixedTimeProvider.DefaultUtcNow));
    }

    private UpdateUserCommandHandler CreateHandler() => new(_users, new InMemoryUserPhotoRepository(), _unitOfWork);

    [Fact]
    public async Task HandleAsync_KeepingOwnEmail_SavesNormalizedChanges()
    {
        var result = await CreateHandler().HandleAsync(
            new UpdateUserCommand(_ana.Id, " Ana Gómez ", " ANA@acme.com "),
            CancellationToken.None);

        Assert.Equal("Ana Gómez", result.Name);
        Assert.Equal("ana@acme.com", result.Email);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, result.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WithAnotherUsersEmail_ThrowsConflictWithoutSaving()
    {
        await Assert.ThrowsAsync<ConflictException>(() => CreateHandler().HandleAsync(
            new UpdateUserCommand(_ana.Id, "Ana", "Luis@Acme.com"),
            CancellationToken.None));

        Assert.Equal("ana@acme.com", _ana.Email);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().HandleAsync(
            new UpdateUserCommand(Guid.NewGuid(), "Ana", "otra@acme.com"),
            CancellationToken.None));
    }
}
