using TaskManager.Domain.Entities.Base;
using TaskManager.Domain.Utilities.Validation;

namespace TaskManager.Domain.Entities.Users;

public class User : Entity
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    private User()
    {
    }

    private User(Guid id, string name, string email, DateTime createdAt)
        : base(id, createdAt)
    {
        Name = name;
        Email = email;
    }

    public static User Create(string name, string email, DateTime createdAt)
    {
        var validName = Guard.RequiredText(name, nameof(Name), NameMaxLength);
        var validEmail = Guard.RequiredText(NormalizeEmail(email), nameof(Email), EmailMaxLength);

        return new User(NewId(), validName, validEmail, createdAt);
    }

    public void Update(string name, string email)
    {
        var validName = Guard.RequiredText(name, nameof(Name), NameMaxLength);
        var validEmail = Guard.RequiredText(NormalizeEmail(email), nameof(Email), EmailMaxLength);

        Name = validName;
        Email = validEmail;
    }

    public static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;
}
