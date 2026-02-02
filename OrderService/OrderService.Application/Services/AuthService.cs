using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrderService.Application.IServices;

namespace OrderService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsIdentity? GetClaimsIdentity()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var identity = user?.Identity as ClaimsIdentity;
        return identity;
    }

    public Guid GetAuthenticatedUserId()
    {
        var userId = GetClaimsIdentity()!.Claims.FirstOrDefault(x => x.Type.Equals("userId"))!.Value;

        return Guid.Parse(userId);
    }

    public string GetAuthenticatedUserRole()
    {
        var userRole = GetClaimsIdentity()!.Claims.FirstOrDefault(x => x.Type.Equals("Role"))!.Value;
        return userRole;
    }
}