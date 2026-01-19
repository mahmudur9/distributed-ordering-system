using Microsoft.AspNetCore.Mvc;

namespace ObjectStoreService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Test");
    }
}