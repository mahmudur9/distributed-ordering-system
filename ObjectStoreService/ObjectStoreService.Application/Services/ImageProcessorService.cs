using Microsoft.AspNetCore.Http;
using ObjectStoreService.Application.IServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ObjectStoreService.Application.Services;

public class ImageProcessorService : IImageProcessorService
{
    public async Task ImageResizeAndSaveAsync(string path, IFormFile file)
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
}