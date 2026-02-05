using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UserService.Application.Abstractions.Logging;
using UserService.Application.Abstractions.Security;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenHandler _tokenHandler;
    private readonly IAppLogger<AuthService> _logger;

    public AuthService(IHttpContextAccessor httpContextAccessor, ITokenHandler tokenHandler, IAppLogger<AuthService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenHandler = tokenHandler;
        _logger = logger;
    }

    public string GenerateToken(Guid userId, string email, string role)
    {
        try
        {
            _logger.LogInformation($"Generating token for user {userId}");
            return _tokenHandler.GenerateToken(userId, email, role);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to generate token for user {userId}");
            throw;
        }
    }

    public Guid GetAuthenticatedUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var identity = user?.Identity as ClaimsIdentity;
        var userId = identity!.Claims.FirstOrDefault(x => x.Type.Equals("userId"))!.Value;

        return Guid.Parse(userId);
    }

    public string GetAuthenticatedUserRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var identity = user?.Identity as ClaimsIdentity;
        var userRole = identity!.Claims.FirstOrDefault(x => x.Type.Equals("Role"))!.Value;
        return userRole;
    }

    public TokenValidationResponse ValidateToken(TokenValidationRequest request)
    {
        return _tokenHandler.ValidateToken(request);
    }
}