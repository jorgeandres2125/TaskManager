using TaskManager.Domain.Entities.Base;
using TaskManager.Domain.Enums.Tasks;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Utilities.Tasks;
using TaskManager.Domain.Utilities.Validation;

namespace TaskManager.Domain.Entities.Tasks;

/// <summary>Tarea asignada a un usuario. Se llama TaskItem para no chocar con System.Threading.Tasks.Task.</summary>
public class TaskItem : Entity
{
    public const int TitleMaxLength = 200;

    public long Number { get; private set; }

    public string Title { get; private set; } = null!;

    public TaskItemStatusEnum Status { get; private set; }

    public Guid UserId { get; private set; }

    public TaskAdditionalInfo? AdditionalInfo { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private TaskItem()
    {
    }

    private TaskItem(Guid id, string title, Guid userId, TaskAdditionalInfo? additionalInfo, DateTime createdAt)
        : base(id, createdAt)
    {
        Title = title;
        UserId = userId;
        AdditionalInfo = additionalInfo;
        Status = TaskItemStatusEnum.Pending;
    }

    public static TaskItem Create(string title, Guid userId, TaskAdditionalInfo? additionalInfo, DateTime createdAt)
    {
        var validTitle = Guard.RequiredText(title, nameof(Title), TitleMaxLength);
        var validUserId = Guard.RequiredId(userId, nameof(UserId));

        return new TaskItem(NewId(), validTitle, validUserId, additionalInfo, createdAt);
    }

    public void Update(string title, Guid userId, TaskAdditionalInfo? additionalInfo, DateTime updatedAt)
    {
        var validTitle = Guard.RequiredText(title, nameof(Title), TitleMaxLength);
        var validUserId = Guard.RequiredId(userId, nameof(UserId));

        Title = validTitle;
        UserId = validUserId;
        AdditionalInfo = additionalInfo;
        UpdatedAt = updatedAt;
    }

    public void ChangeStatus(TaskItemStatusEnum newStatus, DateTime changedAt)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!TaskStatusTransitions.IsAllowed(Status, newStatus))
        {
            throw new InvalidTaskStatusTransitionException(Status, newStatus);
        }

        Status = newStatus;
        UpdatedAt = changedAt;
    }
}
