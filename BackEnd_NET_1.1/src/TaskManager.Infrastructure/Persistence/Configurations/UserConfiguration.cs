using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities.Users;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : EntityConfiguration<User>
{
    protected override string TableName => "Users";

    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Name).HasMaxLength(User.NameMaxLength).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(User.EmailMaxLength).IsRequired();

        builder.HasIndex(user => user.Email)
            .HasDatabaseName("UX_Users_Email")
            .IsUnique();
    }

    protected override void ConfigureTable(TableBuilder<User> table)
    {
        table.HasCheckConstraint("CK_Users_Name_NotBlank", "LEN(TRIM([Name])) > 0");
        table.HasCheckConstraint("CK_Users_Email_NotBlank", "LEN(TRIM([Email])) > 0");
    }
}
