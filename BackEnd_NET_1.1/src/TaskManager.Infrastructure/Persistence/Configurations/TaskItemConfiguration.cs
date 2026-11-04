using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Infrastructure.Persistence.Converters;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal class TaskItemConfiguration : EntityConfiguration<TaskItem>
{
    public const string PriorityColumn = "Priority";

    public const string NumberSequence = "TaskNumberSequence";

    private const int StatusMaxLength = 20;
    private const int PriorityMaxLength = 10;

    protected override string TableName => "Tasks";

    protected override void ConfigureEntity(EntityTypeBuilder<TaskItem> builder)
    {
        builder.Property(task => task.Number)
            .IsRequired()
            .HasDefaultValueSql($"NEXT VALUE FOR dbo.{NumberSequence}")
            .ValueGeneratedOnAdd();

        builder.HasIndex(task => task.Number)
            .HasDatabaseName("UX_Tasks_Number")
            .IsUnique();

        builder.Property(task => task.Title).HasMaxLength(TaskItem.TitleMaxLength).IsRequired();

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(task => task.UserId)
            .HasConstraintName("FK_Tasks_Users_UserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(task => task.AdditionalInfo)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                info => TaskAdditionalInfoJson.Serialize(info),
                json => TaskAdditionalInfoJson.Deserialize(json),
                new ValueComparer<TaskAdditionalInfo?>(
                    (left, right) => TaskAdditionalInfoJson.Serialize(left) == TaskAdditionalInfoJson.Serialize(right),
                    info => info == null ? 0 : TaskAdditionalInfoJson.Serialize(info)!.GetHashCode(),
                    info => TaskAdditionalInfoJson.Deserialize(TaskAdditionalInfoJson.Serialize(info))));

        builder.Property<string?>(PriorityColumn)
            .HasMaxLength(PriorityMaxLength)
            .HasComputedColumnSql($"CAST(JSON_VALUE([AdditionalInfo], '$.priority') AS nvarchar({PriorityMaxLength}))");

        builder.HasIndex(task => new { task.UserId, task.Status, task.CreatedAt })
            .HasDatabaseName("IX_Tasks_UserId_Status_CreatedAt")
            .IsDescending(false, false, true);

        builder.HasIndex(task => new { task.Status, task.CreatedAt })
            .HasDatabaseName("IX_Tasks_Status_CreatedAt")
            .IsDescending(false, true);

        builder.HasIndex(PriorityColumn).HasDatabaseName("IX_Tasks_Priority");
    }

    protected override void ConfigureTable(TableBuilder<TaskItem> table)
    {
        table.HasCheckConstraint("CK_Tasks_Title_NotBlank", "LEN(TRIM([Title])) > 0");
        table.HasCheckConstraint("CK_Tasks_Status", $"[Status] IN ({SqlList<TaskItemStatusEnum>()})");

        table.HasCheckConstraint("CK_Tasks_AdditionalInfo_IsJson", "[AdditionalInfo] IS NULL OR ISJSON([AdditionalInfo], OBJECT) = 1");
        table.HasCheckConstraint(
            "CK_Tasks_AdditionalInfo_Priority",
            $"JSON_VALUE([AdditionalInfo], '$.priority') IS NULL OR JSON_VALUE([AdditionalInfo], '$.priority') IN ({SqlList<TaskPriorityEnum>()})");
    }

    private static string SqlList<TEnum>() where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"N'{name}'"));
}
