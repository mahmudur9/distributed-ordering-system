using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Application.IServices;

public interface IRoleService
{
    Task<PaginatedResponse<RoleResponse>> GetAllRolesAsync(GetAllRolesFilter filter);
}