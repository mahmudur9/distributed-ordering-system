using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.API.Controllers;

[Authorize]
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
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersFilter filter)
    {
        return Ok(await _userService.GetAllUsersAsync(filter));
    }

    [HttpPost("Register"), AllowAnonymous]
    public async Task<IActionResult> Register(UserRequest userRequest)
    {
        await _userService.CreateUserAsync(userRequest);
        return Ok("User registration successful");
    }

    [HttpPost("Login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        return Ok(await _userService.LoginAsync(loginRequest));
    }

    [HttpGet("Authenticate")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Authenticate()
    {
        return Ok(await _userService.AuthenticateAsync());
    }
}