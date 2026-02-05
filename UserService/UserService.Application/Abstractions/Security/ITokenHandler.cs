using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Application.Abstractions.Security;

public interface ITokenHandler
{
    string GenerateToken(Guid userId, string email, string role);
    TokenValidationResponse ValidateToken(TokenValidationRequest request);
}