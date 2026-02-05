using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Application.IServices;

public interface IAuthService
{
    string GenerateToken(Guid userId, string email, string role);
    Guid GetAuthenticatedUserId();
    string GetAuthenticatedUserRole();
    TokenValidationResponse ValidateToken(TokenValidationRequest request);
}