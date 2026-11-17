using TaskManager.Application.Abstractions.Tasks;
using TaskManager.Application.Models.Pagination;
using TaskManager.Application.Models.Tasks;
using TaskManager.Domain.Entities.Tasks;
using TaskManager.Domain.Interfaces.Tasks;

namespace TaskManager.Tests.TestDoubles;

/// <summary>Implementa escritura y lectura sobre la misma lista para probar los handlers de tareas.</summary>
internal class InMemoryTaskRepository(InMemoryUserRepository users) : ITaskRepository, ITaskReadRepository
{
    public List<TaskItem> Tasks { get; } = [];

    public void Add(TaskItem task) => Tasks.Add(task);

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Tasks.FirstOrDefault(task => task.Id == id));

    public Task<PagedList<TaskReadModel>> SearchAsync(TaskSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var filtered = Tasks
            .Where(task => criteria.TaskId is null || task.Id == criteria.TaskId)
            .Where(task => criteria.UserId is null || task.UserId == criteria.UserId)
            .Where(task => criteria.Status is null || task.Status == criteria.Status)
            .ToList();

        var page = criteria.Page is null
            ? filtered
            : filtered.Skip(criteria.Page.Skip).Take(criteria.Page.PageSize);

        var items = page
            .Select(task => new TaskReadModel(
                task.Id,
                task.Number,
                task.Title,
                task.Status,
                task.UserId,
                users.Users.First(user => user.Id == task.UserId).Name,
                task.CreatedAt,
                task.UpdatedAt,
                task.AdditionalInfo))
            .ToList();

        return Task.FromResult(new PagedList<TaskReadModel>(items, filtered.Count));
    }
}
