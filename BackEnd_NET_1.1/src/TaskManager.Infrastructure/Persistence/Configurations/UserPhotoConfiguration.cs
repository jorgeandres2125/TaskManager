using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities.Users;

namespace TaskManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabla UserPhotos: la foto en VARBINARY(MAX) (el tipo BLOB de SQL Server) con el nombre del archivo y su extensión.
/// Un usuario tiene como máximo una foto (índice único sobre UserId) y se borra con él (cascada).
/// </summary>
internal class UserPhotoConfiguration : EntityConfiguration<UserPhoto>
{
    protected override string TableName => "UserPhotos";

    protected override void ConfigureEntity(EntityTypeBuilder<UserPhoto> builder)
    {
        builder.Property(photo => photo.FileName).HasMaxLength(UserPhoto.FileNameMaxLength).IsRequired();
        builder.Property(photo => photo.Extension).HasMaxLength(UserPhoto.ExtensionMaxLength).IsRequired();
        builder.Property(photo => photo.ContentType).HasMaxLength(UserPhoto.ContentTypeMaxLength).IsRequired();
        builder.Property(photo => photo.SizeBytes).IsRequired();
        builder.Property(photo => photo.Content).HasColumnType("varbinary(max)").IsRequired();
        builder.Ignore(photo => photo.Version);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(photo => photo.UserId)
            .HasConstraintName("FK_UserPhotos_Users_UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(photo => photo.UserId)
            .HasDatabaseName("UX_UserPhotos_UserId")
            .IsUnique();
    }

    protected override void ConfigureTable(TableBuilder<UserPhoto> table)
    {
        table.HasCheckConstraint("CK_UserPhotos_FileName_NotBlank", "LEN(TRIM([FileName])) > 0");
        table.HasCheckConstraint("CK_UserPhotos_Extension_Format", "[Extension] LIKE N'.%'");
        table.HasCheckConstraint("CK_UserPhotos_SizeBytes", $"[SizeBytes] > 0 AND [SizeBytes] <= {UserPhoto.MaxSizeBytes}");
        table.HasCheckConstraint("CK_UserPhotos_SizeBytes_MatchesContent", "[SizeBytes] = DATALENGTH([Content])");
    }
}
