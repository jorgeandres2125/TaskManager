using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities.Base;

namespace TaskManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración común de toda entidad: Id GUID y CreatedAt obligatorio, ambos indexados.
/// La PK es NONCLUSTERED y el índice CLUSTERED va sobre CreatedAt: un GUID como clave clustered
/// fragmentaría la tabla, mientras que CreatedAt crece de forma natural y acelera el orden por fecha.
/// </summary>
internal abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName, ConfigureTable);

        builder.HasKey(entity => entity.Id)
            .HasName($"PK_{TableName}")
            .IsClustered(false);

        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property(entity => entity.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(entity => entity.CreatedAt)
            .HasDatabaseName($"CIX_{TableName}_CreatedAt")
            .IsClustered();

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);

    protected virtual void ConfigureTable(TableBuilder<TEntity> table)
    {
    }
}
