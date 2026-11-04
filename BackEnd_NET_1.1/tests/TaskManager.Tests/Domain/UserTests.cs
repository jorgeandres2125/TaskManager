using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Exceptions;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_NormalizesNameAndEmail()
    {
        var user = User.Create("  Ana Gómez ", "  Ana.Gomez@Acme.COM ", FixedTimeProvider.DefaultUtcNow);

        Assert.Equal("Ana Gómez", user.Name);
        Assert.Equal("ana.gomez@acme.com", user.Email);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, user.CreatedAt);
    }

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => User.Create(" ", "ana@acme.com", FixedTimeProvider.DefaultUtcNow));

        Assert.Equal(nameof(User.Name), exception.PropertyName);
    }
}
