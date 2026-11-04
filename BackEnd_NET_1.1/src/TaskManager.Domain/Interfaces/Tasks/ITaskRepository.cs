using TaskManager.Domain.Entities.Tasks;

namespace TaskManager.Domain.Interfaces.Tasks;

public interface ITaskRepository
{
    void Add(TaskItem task);

    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
