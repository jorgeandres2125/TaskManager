using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.Validators.Tasks;
using TaskManager.Domain.Entities.Tasks;

namespace TaskManager.Tests.Application;

public class PatchTaskAdditionalInfoCommandValidatorTests
{
    private static async Task<IEnumerable<string>> InvalidPropertiesAsync(PatchTaskAdditionalInfoCommand command)
    {
        var result = await new PatchTaskAdditionalInfoCommandValidator().ValidateAsync(command);
        return result.Errors.Select(error => error.PropertyName).Distinct();
    }

    private static PatchTaskAdditionalInfoCommand Command(
        string? priority = null,
        bool clearPriority = false,
        IReadOnlyList<string>? addTags = null,
        IReadOnlyList<string>? removeTags = null,
        IReadOnlyDictionary<string, string>? setMetadata = null,
        IReadOnlyList<string>? removeMetadata = null) =>
        new(Guid.NewGuid(), priority, clearPriority, null, false, addTags, removeTags, setMetadata, removeMetadata);

    [Fact]
    public async Task ValidCommand_HasNoErrors()
    {
        Assert.Empty(await InvalidPropertiesAsync(Command(
            priority: "High",
            addTags: ["api"],
            removeTags: ["backend"],
            setMetadata: new Dictionary<string, string> { ["ticket_id"] = "DB-42" },
            removeMetadata: ["sprint-1"])));
    }

    [Fact]
    public async Task WithoutChanges_IsInvalid()
    {
        Assert.Contains("AdditionalInfo", await InvalidPropertiesAsync(Command(addTags: [])));
    }

    [Fact]
    public async Task UnknownPriority_IsInvalid()
    {
        Assert.Contains(nameof(PatchTaskAdditionalInfoCommand.Priority), await InvalidPropertiesAsync(Command(priority: "Urgent")));
    }

    [Fact]
    public async Task SettingAndClearingPriority_IsInvalid()
    {
        Assert.Contains(
            nameof(PatchTaskAdditionalInfoCommand.ClearPriority),
            await InvalidPropertiesAsync(Command(priority: "Low", clearPriority: true)));
    }

    [Fact]
    public async Task TooManyTags_IsInvalid()
    {
        var tags = Enumerable.Range(0, TaskAdditionalInfo.MaxTags + 1).Select(index => $"tag{index}").ToArray();

        Assert.Contains(nameof(PatchTaskAdditionalInfoCommand.AddTags), await InvalidPropertiesAsync(Command(addTags: tags)));
    }

    [Fact]
    public async Task AddingAndRemovingSameTag_IsInvalid()
    {
        Assert.NotEmpty(await InvalidPropertiesAsync(Command(addTags: ["Api"], removeTags: ["api"])));
    }

    [Theory]
    [InlineData("con espacio")]
    [InlineData("comilla\"")]
    [InlineData("año")]
    [InlineData("")]
    public async Task MetadataKeyOutsideAllowedCharacters_IsInvalid(string key)
    {
        Assert.NotEmpty(await InvalidPropertiesAsync(Command(setMetadata: new Dictionary<string, string> { [key] = "valor" })));
        Assert.NotEmpty(await InvalidPropertiesAsync(Command(removeMetadata: [key])));
    }

    [Fact]
    public async Task SettingAndRemovingSameMetadataKey_IsInvalid()
    {
        Assert.NotEmpty(await InvalidPropertiesAsync(Command(
            setMetadata: new Dictionary<string, string> { ["ticket"] = "DB-42" },
            removeMetadata: ["ticket"])));
    }
}
