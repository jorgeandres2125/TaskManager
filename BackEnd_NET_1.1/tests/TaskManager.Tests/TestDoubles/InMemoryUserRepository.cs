using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Tests.TestDoubles;

internal class InMemoryUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public void Add(User user) => Users.Add(user);

    public Task<IReadOnlyList<User>> ListAsync(string? search, int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<User>>(
            Filter(search).OrderBy(user => user.Name).ThenBy(user => user.Id).Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        Task.FromResult(Filter(search).Count());

    private IEnumerable<User> Filter(string? search) =>
        search is null
            ? Users
            : Users.Where(user =>
                user.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                user.Email.Contains(search, StringComparison.OrdinalIgnoreCase));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.FirstOrDefault(user => user.Id == id));

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) => GetByIdAsync(id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(user => user.Id == id));

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? exceptUserId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(user => user.Email == normalizedEmail && user.Id != exceptUserId));
}
