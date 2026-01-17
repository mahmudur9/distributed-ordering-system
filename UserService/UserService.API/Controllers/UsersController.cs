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
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), StatusCodes.Status200OK)]
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
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

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest updateUserRequest)
    {
        await  _userService.UpdateUserAsync(id, updateUserRequest);
        return Ok("User updated successfully");
    }

    [HttpPut("UpdatePassword/{id}")]
    public async Task<IActionResult> UpdatePassword(Guid id, UpdatePasswordRequest updatePasswordRequest)
    {
        await _userService.UpdatePasswordAsync(id, updatePasswordRequest);
        return Ok("Password updated successfully");
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok("User deleted successfully");
    }
}