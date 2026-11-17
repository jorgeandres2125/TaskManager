using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories.EntityFramework;

internal class UserEFRepository(TaskManagerDbContext context) : IUserRepository
{
    public void Add(User user) => context.Users.Add(user);

    public async Task<IReadOnlyList<User>> ListAsync(string? search, int skip, int take, CancellationToken cancellationToken) =>
        await Filter(context.Users.AsNoTracking(), search)
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        Filter(context.Users.AsNoTracking(), search).CountAsync(cancellationToken);

    private static IQueryable<User> Filter(IQueryable<User> users, string? search) =>
        search is null
            ? users
            : users.Where(user => user.Name.Contains(search) || user.Email.Contains(search));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking().AnyAsync(user => user.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? exceptUserId, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail && user.Id != exceptUserId, cancellationToken);
}
