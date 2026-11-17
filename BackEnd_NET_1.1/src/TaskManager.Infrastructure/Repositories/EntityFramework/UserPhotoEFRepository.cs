using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories.EntityFramework;

internal class UserPhotoEFRepository(TaskManagerDbContext context) : IUserPhotoRepository
{
    public void Add(UserPhoto photo) => context.UserPhotos.Add(photo);

    public void Remove(UserPhoto photo) => context.UserPhotos.Remove(photo);

    public Task<UserPhoto?> GetForUpdateAsync(Guid userId, CancellationToken cancellationToken) =>
        context.UserPhotos.FirstOrDefaultAsync(photo => photo.UserId == userId, cancellationToken);

    public Task<UserPhoto?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        context.UserPhotos.AsNoTracking().FirstOrDefaultAsync(photo => photo.UserId == userId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, DateTime>> GetVersionsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, DateTime>();
        }

        return await context.UserPhotos.AsNoTracking()
            .Where(photo => userIds.Contains(photo.UserId))
            .Select(photo => new { photo.UserId, Version = photo.UpdatedAt ?? photo.CreatedAt })
            .ToDictionaryAsync(photo => photo.UserId, photo => photo.Version, cancellationToken);
    }
}
