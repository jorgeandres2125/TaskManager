using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.Validators.Users;
using TaskManager.Domain.Entities.Users;
using TaskManager.Tests.TestDoubles;

namespace TaskManager.Tests.Application;

public class UploadUserPhotoCommandValidatorTests
{
    private static IEnumerable<string> InvalidProperties(UploadUserPhotoCommand command) =>
        new UploadUserPhotoCommandValidator().Validate(command).Errors.Select(error => error.PropertyName).Distinct();

    [Theory]
    [InlineData("ana.jpg")]
    [InlineData("ANA.JPEG")]
    public void Jpeg_IsValid(string fileName)
    {
        Assert.Empty(InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), fileName, TestImages.Jpeg)));
    }

    [Fact]
    public void PngAndWebp_AreValid()
    {
        Assert.Empty(InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "a.png", TestImages.Png)));
        Assert.Empty(InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "a.webp", TestImages.Webp)));
    }

    [Fact]
    public void MissingFile_IsInvalid()
    {
        var invalid = InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), null, null));

        Assert.Contains(nameof(UploadUserPhotoCommand.FileName), invalid);
        Assert.Contains(nameof(UploadUserPhotoCommand.Content), invalid);
    }

    [Fact]
    public void UnsupportedExtension_IsInvalid()
    {
        Assert.Contains(
            nameof(UploadUserPhotoCommand.FileName),
            InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "ana.gif", TestImages.Jpeg)));
    }

    [Fact]
    public void FileThatIsNotAnImage_IsInvalid()
    {
        Assert.Contains(
            nameof(UploadUserPhotoCommand.Content),
            InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "documento.jpg", TestImages.Pdf)));
    }

    [Fact]
    public void ExtensionThatDoesNotMatchContent_IsInvalid()
    {
        Assert.Contains(
            nameof(UploadUserPhotoCommand.Content),
            InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "ana.png", TestImages.Jpeg)));
    }

    [Fact]
    public void TooLargeFile_IsInvalid()
    {
        var content = new byte[UserPhoto.MaxSizeBytes + 1];
        TestImages.Jpeg.CopyTo(content, 0);

        Assert.Contains(
            nameof(UploadUserPhotoCommand.Content),
            InvalidProperties(new UploadUserPhotoCommand(Guid.NewGuid(), "ana.jpg", content)));
    }
}
