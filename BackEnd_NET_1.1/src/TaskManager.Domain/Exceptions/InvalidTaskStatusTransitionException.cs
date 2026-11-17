using TaskManager.Domain.Enums.Tasks;

namespace TaskManager.Domain.Exceptions;

public class InvalidTaskStatusTransitionException(TaskItemStatusEnum from, TaskItemStatusEnum to)
    : DomainException($"No se permite cambiar una tarea de {from} a {to}.")
{
    public TaskItemStatusEnum From { get; } = from;

    public TaskItemStatusEnum To { get; } = to;
}
