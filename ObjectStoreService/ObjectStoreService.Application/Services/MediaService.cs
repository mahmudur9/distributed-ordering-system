using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ObjectStoreService.Application.IServices;
using ObjectStoreService.Application.Requests;
using ObjectStoreService.Application.Responses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ObjectStoreService.Application.Services;

public class MediaService : IMediaService
{
    private readonly ILogger<MediaService> _logger;

    public MediaService(ILogger<MediaService> logger)
    {
        _logger = logger;
    }

    private async Task ImageResizeAndSaveAsync(string path, IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        using var image = await Image.LoadAsync(stream);

        int maxWidth = 600;
        int maxHeight = 600;

        int originalWidth = image.Width;
        int originalHeight = image.Height;

        double ratioX = (double)maxWidth / originalWidth;
        double ratioY = (double)maxHeight / originalHeight;
        double ratio = Math.Min(ratioX, ratioY);

        int newWidth = (int)(originalWidth * ratio);
        int newHeight = (int)(originalHeight * ratio);

        image.Mutate(x => x.Resize(newWidth, newHeight));

        await image.SaveAsync(path);
    }

    public async Task<MediaResponse> UploadAsync(MediaRequest mediaRequest)
    {
        try
        {
            _logger.LogInformation("Uploading file");
            var file = mediaRequest.MediaFile;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

            // Ensure the directory exists
            string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(wwwrootPath);

            await ImageResizeAndSaveAsync(path, file);

            // string host = _httpContextAccessor.HttpContext!.Request.Host.Host;
            var response = new MediaResponse()
            {
                Url = "http://localhost:8005" + "/" + fileName,
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload");
            throw;
        }
    }

    public async Task DeleteAsync(MediaDeleteRequest mediaDeleteRequest)
    {
        try
        {
            _logger.LogInformation("Deleting file");
            string fileName = mediaDeleteRequest.Url.Split('/').Last();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
            else
            {
                throw new FileNotFoundException("File not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete");
            throw;
        }
    }
}