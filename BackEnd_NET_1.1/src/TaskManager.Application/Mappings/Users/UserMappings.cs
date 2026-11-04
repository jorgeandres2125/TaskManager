using TaskManager.Application.DTOs.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Domain.Interfaces.Users;

namespace TaskManager.Application.Mappings.Users;

internal static class UserMappings
{
    public static UserDto ToDto(this User user, DateTime? photoUpdatedAt = null) =>
        new(user.Id, user.Name, user.Email, user.CreatedAt, photoUpdatedAt);

    public static async Task<IReadOnlyList<UserDto>> ToDtosAsync(
        this IReadOnlyCollection<User> users,
        IUserPhotoRepository photos,
        CancellationToken cancellationToken)
    {
        var versions = await photos.GetVersionsAsync(users.Select(user => user.Id).ToArray(), cancellationToken);
        return users.Select(user => user.ToDto(versions.TryGetValue(user.Id, out var version) ? version : null)).ToArray();
    }

    public static async Task<UserDto> ToDtoAsync(this User user, IUserPhotoRepository photos, CancellationToken cancellationToken) =>
        (await new[] { user }.ToDtosAsync(photos, cancellationToken))[0];
}
