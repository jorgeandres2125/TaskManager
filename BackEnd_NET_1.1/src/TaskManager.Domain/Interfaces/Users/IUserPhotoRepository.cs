using TaskManager.Domain.Entities.Users;

namespace TaskManager.Domain.Interfaces.Users;

public interface IUserPhotoRepository
{
    void Add(UserPhoto photo);

    void Remove(UserPhoto photo);

    Task<UserPhoto?> GetForUpdateAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserPhoto?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, DateTime>> GetVersionsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);
}
