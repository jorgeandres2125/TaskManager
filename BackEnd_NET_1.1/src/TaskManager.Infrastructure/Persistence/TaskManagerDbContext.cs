using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Infrastructure.Persistence.Configurations;
using TaskManager.Infrastructure.Persistence.Converters;

namespace TaskManager.Infrastructure.Persistence;

public class TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(TaskItemConfiguration.NumberSequence, "dbo").StartsAt(1).IncrementsBy(1);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagerDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
}
