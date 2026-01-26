using Microsoft.AspNetCore.Mvc;
using ObjectStoreService.Application.IServices;
using ObjectStoreService.Application.Requests;

namespace ObjectStoreService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> Upload([FromForm] MediaRequest  mediaRequest)
    {
        return Ok(await _mediaService.UploadAsync(mediaRequest));
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(MediaDeleteRequest mediaDeleteRequest)
    {
        await _mediaService.DeleteAsync(mediaDeleteRequest);
        return Ok("Deleted");
    }
}