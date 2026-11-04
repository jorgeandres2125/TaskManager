using TaskManager.Application.Exceptions;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.UseCases.Handlers.Users;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class UserPhotoCommandHandlerTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryUserPhotoRepository _photos = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FixedTimeProvider _time = new();
    private readonly User _ana = User.Create("Ana", "ana@acme.com", FixedTimeProvider.DefaultUtcNow);

    public UserPhotoCommandHandlerTests() => _users.Add(_ana);

    private UploadUserPhotoCommandHandler UploadHandler() => new(_users, _photos, _unitOfWork, _time);

    [Fact]
    public async Task Upload_SavesFileNameExtensionAndDetectedContentType()
    {
        var result = await UploadHandler().HandleAsync(
            new UploadUserPhotoCommand(_ana.Id, @"C:\fotos\Ana Perfil.JPG", TestImages.Jpeg),
            CancellationToken.None);

        var photo = Assert.Single(_photos.Photos);
        Assert.Equal("Ana Perfil.JPG", photo.FileName);
        Assert.Equal(".jpg", photo.Extension);
        Assert.Equal("image/jpeg", photo.ContentType);
        Assert.Equal(TestImages.Jpeg.Length, photo.SizeBytes);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow, result.PhotoUpdatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Upload_Twice_ReplacesThePhoto()
    {
        await UploadHandler().HandleAsync(new UploadUserPhotoCommand(_ana.Id, "a.jpg", TestImages.Jpeg), CancellationToken.None);
        _time.Advance(TimeSpan.FromMinutes(5));

        var result = await UploadHandler().HandleAsync(new UploadUserPhotoCommand(_ana.Id, "b.png", TestImages.Png), CancellationToken.None);

        var photo = Assert.Single(_photos.Photos);
        Assert.Equal("b.png", photo.FileName);
        Assert.Equal("image/png", photo.ContentType);
        Assert.Equal(FixedTimeProvider.DefaultUtcNow.AddMinutes(5), result.PhotoUpdatedAt);
    }

    [Fact]
    public async Task Upload_UnknownUser_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => UploadHandler().HandleAsync(
            new UploadUserPhotoCommand(Guid.NewGuid(), "a.jpg", TestImages.Jpeg),
            CancellationToken.None));

        Assert.Empty(_photos.Photos);
    }

    [Fact]
    public async Task Delete_RemovesThePhoto_AndIsIdempotent()
    {
        await UploadHandler().HandleAsync(new UploadUserPhotoCommand(_ana.Id, "a.jpg", TestImages.Jpeg), CancellationToken.None);
        var handler = new DeleteUserPhotoCommandHandler(_users, _photos, _unitOfWork);

        var result = await handler.HandleAsync(new DeleteUserPhotoCommand(_ana.Id), CancellationToken.None);
        await handler.HandleAsync(new DeleteUserPhotoCommand(_ana.Id), CancellationToken.None);

        Assert.Empty(_photos.Photos);
        Assert.Null(result.PhotoUpdatedAt);
    }

    [Fact]
    public async Task GetPhoto_WithoutPhoto_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetUserPhotoQueryHandler(_photos).HandleAsync(new GetUserPhotoQuery(_ana.Id), CancellationToken.None));
    }
}
