using System.Security.Claims;
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

    [HttpPost("Register")]
    public async Task<IActionResult> Register(UserRequest userRequest)
    {
        await _userService.CreateUserAsync(userRequest);
        return Ok("User registration successful");
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        return Ok(await _userService.LoginAsync(loginRequest));
    }

    [HttpGet("Authenticate")]
    public async Task<IActionResult> Authenticate()
    {
        return Ok(await _userService.AuthenticateAsync());
    }
}