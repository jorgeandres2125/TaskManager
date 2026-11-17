using TaskManager.Domain.Entities.Users;

namespace TaskManager.Domain.Interfaces.Users;

public interface IUserRepository
{
    void Add(User user);

    Task<IReadOnlyList<User>> ListAsync(string? search, int skip, int take, CancellationToken cancellationToken);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? exceptUserId, CancellationToken cancellationToken);
}
