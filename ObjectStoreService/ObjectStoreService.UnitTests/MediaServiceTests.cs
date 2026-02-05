using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ObjectStoreService.Application.IServices;
using ObjectStoreService.Application.Requests;
using ObjectStoreService.Application.Services;
using ObjectStoreService.Domain.ILogging;

namespace ObjectStoreService.UnitTests;

public class MediaServiceTests
{
    private readonly Mock<IAppLogger<MediaService>> _mockLogger = new();
    private readonly Mock<IImageProcessorService> _imageProcessorServiceMock = new();
    private readonly MediaService _mediaService;

    public MediaServiceTests()
    {
        _mediaService =  new MediaService(_mockLogger.Object, _imageProcessorServiceMock.Object);
    }

    [Fact]
    public async Task UploadAsync_Should_Return_MediaResponse_When_Upload_Succeeds()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.OpenReadStream())
            .Returns(new MemoryStream([1, 2, 3]));

        var request = new MediaRequest
        {
            MediaFile = fileMock.Object
        };

        _imageProcessorServiceMock.Setup(x => x.ImageResizeAndSaveAsync(
            It.IsAny<string>(), It.IsAny<IFormFile>()
        )).Returns(Task.CompletedTask);
        
        // Act
        var result = await _mediaService.UploadAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("http://localhost:8005/", result.Url);

        _imageProcessorServiceMock.Verify(
            s => s.ImageResizeAndSaveAsync(It.IsAny<string>(), fileMock.Object),
            Times.Once);
        _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task UploadAsync_Should_Throw_When_Save_Fails()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        var request = new MediaRequest
        {
            MediaFile = fileMock.Object
        };

        _imageProcessorServiceMock.Setup(x => x.ImageResizeAndSaveAsync(
            It.IsAny<string>(), It.IsAny<IFormFile>()
        )).ThrowsAsync(new IOException("disk error"));
        
        // Act
        Func<Task> act = async () => await _mediaService.UploadAsync(request);
        
        // Assert
        await act.Should().ThrowAsync<IOException>().WithMessage("disk error");

        _imageProcessorServiceMock.Verify(
            s => s.ImageResizeAndSaveAsync(It.IsAny<string>(), fileMock.Object),
            Times.Once);
        _mockLogger.Verify(x => x.LogError(It.IsAny<IOException>(), It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_Should_Delete_File_When_Exists()
    {
        // Arrange
        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(wwwroot);

        var fileName = $"{Guid.NewGuid()}.txt";
        var path = Path.Combine(wwwroot, fileName);

        await File.WriteAllTextAsync(path, "test");

        var request = new MediaDeleteRequest
        {
            Url = $"http://localhost:8005/{fileName}"
        };

        
        // Act
        await _mediaService.DeleteAsync(request);
        
        // Assert
        _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_Should_throw_When_File_Does_Not_Exist()
    {
        // Arrange
        var request = new MediaDeleteRequest
        {
            Url = "http://localhost:8005/missing.txt"
        };
        
        // Act
        Func<Task> act = async () => await _mediaService.DeleteAsync(request);
        
        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>().WithMessage("File not found");
        _mockLogger.Verify(x => x.LogError(It.IsAny<FileNotFoundException>(), 
            It.IsAny<string>()), Times.Once);
    }
}