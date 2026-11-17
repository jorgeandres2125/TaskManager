using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Validators.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class CreateTaskCommandValidatorTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly User _existingUser = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);

    public CreateTaskCommandValidatorTests() => _users.Add(_existingUser);

    private async Task<IEnumerable<string>> InvalidPropertiesAsync(CreateTaskCommand command)
    {
        var result = await new CreateTaskCommandValidator(_users).ValidateAsync(command);
        return result.Errors.Select(error => error.PropertyName).Distinct();
    }

    [Fact]
    public async Task ValidCommand_HasNoErrors()
    {
        var command = new CreateTaskCommand(
            "Preparar demo",
            _existingUser.Id,
            new TaskAdditionalInfoDto("High", new DateOnly(2026, 11, 20), ["demo"], new Dictionary<string, string> { ["cliente"] = "ACME" }));

        Assert.Empty(await InvalidPropertiesAsync(command));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MissingTitle_IsInvalid(string? title)
    {
        Assert.Contains(nameof(CreateTaskCommand.Title), await InvalidPropertiesAsync(new CreateTaskCommand(title, _existingUser.Id, null)));
    }

    [Fact]
    public async Task MissingUser_IsInvalid()
    {
        Assert.Contains(nameof(CreateTaskCommand.UserId), await InvalidPropertiesAsync(new CreateTaskCommand("Título", null, null)));
    }

    [Fact]
    public async Task UnknownUser_IsInvalid()
    {
        Assert.Contains(nameof(CreateTaskCommand.UserId), await InvalidPropertiesAsync(new CreateTaskCommand("Título", Guid.NewGuid(), null)));
    }

    [Theory]
    [InlineData("Urgent")]
    [InlineData("1")]
    public async Task UnknownPriority_IsInvalid(string priority)
    {
        var command = new CreateTaskCommand("Título", _existingUser.Id, new TaskAdditionalInfoDto(priority, null, null, null));

        Assert.Contains("AdditionalInfo.Priority", await InvalidPropertiesAsync(command));
    }
}
