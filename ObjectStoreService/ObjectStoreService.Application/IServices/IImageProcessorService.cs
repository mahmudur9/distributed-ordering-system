using Microsoft.AspNetCore.Http;

namespace ObjectStoreService.Application.IServices;

public interface IImageProcessorService
{
    Task ImageResizeAndSaveAsync(string path, IFormFile file);
}