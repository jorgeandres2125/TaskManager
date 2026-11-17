using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Validators.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class UpdateTaskCommandValidatorTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly User _existingUser = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);

    public UpdateTaskCommandValidatorTests() => _users.Add(_existingUser);

    private async Task<IEnumerable<string>> InvalidPropertiesAsync(UpdateTaskCommand command)
    {
        var result = await new UpdateTaskCommandValidator(_users).ValidateAsync(command);
        return result.Errors.Select(error => error.PropertyName).Distinct();
    }

    [Fact]
    public async Task ValidCommand_HasNoErrors()
    {
        Assert.Empty(await InvalidPropertiesAsync(new UpdateTaskCommand(Guid.NewGuid(), "Título", _existingUser.Id, null)));
    }

    [Fact]
    public async Task MissingTitle_IsInvalid()
    {
        Assert.Contains(
            nameof(UpdateTaskCommand.Title),
            await InvalidPropertiesAsync(new UpdateTaskCommand(Guid.NewGuid(), " ", _existingUser.Id, null)));
    }

    [Fact]
    public async Task UnknownUser_IsInvalid()
    {
        Assert.Contains(
            nameof(UpdateTaskCommand.UserId),
            await InvalidPropertiesAsync(new UpdateTaskCommand(Guid.NewGuid(), "Título", Guid.NewGuid(), null)));
    }
}
