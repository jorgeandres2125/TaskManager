using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Interfaces.Tasks;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories.EntityFramework;

internal class TaskEFRepository(TaskManagerDbContext context) : ITaskRepository
{
    public void Add(TaskItem task) => context.Tasks.Add(task);

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
}
