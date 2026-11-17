using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Domain.Utilities.Tasks;

/// <summary>
/// Máquina de estados de una tarea. Pending → Done está prohibida por el enunciado;
/// Done → Pending también se prohíbe para que una tarea reabierta pase por InProgress.
/// </summary>
public static class TaskStatusTransitions
{
    private static readonly IReadOnlyDictionary<TaskItemStatusEnum, TaskItemStatusEnum[]> AllowedTargets =
        new Dictionary<TaskItemStatusEnum, TaskItemStatusEnum[]>
        {
            [TaskItemStatusEnum.Pending] = [TaskItemStatusEnum.InProgress],
            [TaskItemStatusEnum.InProgress] = [TaskItemStatusEnum.Pending, TaskItemStatusEnum.Done],
            [TaskItemStatusEnum.Done] = [TaskItemStatusEnum.InProgress],
        };

    public static IReadOnlyList<TaskItemStatusEnum> GetAllowedTargets(TaskItemStatusEnum from) => AllowedTargets[from];

    public static bool IsAllowed(TaskItemStatusEnum from, TaskItemStatusEnum to) => AllowedTargets[from].Contains(to);
}
