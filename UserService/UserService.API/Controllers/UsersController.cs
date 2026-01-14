using Microsoft.AspNetCore.Mvc;
using UserService.Application.IServices;
using UserService.Application.Requests;

namespace UserService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(GetAllUsersFilter filter)
    {
        return Ok(await _userService.GetAllUsersAsync(filter));
    }
}