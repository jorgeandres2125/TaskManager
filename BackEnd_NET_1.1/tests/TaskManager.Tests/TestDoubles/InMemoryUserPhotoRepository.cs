using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Tests.TestDoubles;

internal class InMemoryUserPhotoRepository : IUserPhotoRepository
{
    public List<UserPhoto> Photos { get; } = [];

    public void Add(UserPhoto photo) => Photos.Add(photo);

    public void Remove(UserPhoto photo) => Photos.Remove(photo);

    public Task<UserPhoto?> GetForUpdateAsync(Guid userId, CancellationToken cancellationToken) => GetAsync(userId, cancellationToken);

    public Task<UserPhoto?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Photos.FirstOrDefault(photo => photo.UserId == userId));

    public Task<IReadOnlyDictionary<Guid, DateTime>> GetVersionsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, DateTime>>(
            Photos.Where(photo => userIds.Contains(photo.UserId)).ToDictionary(photo => photo.UserId, photo => photo.Version));
}
