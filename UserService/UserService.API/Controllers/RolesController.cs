using Microsoft.AspNetCore.Mvc;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllRolesFilter filter)
    {
        return Ok(await _roleService.GetAllRolesAsync(filter));
    }
}