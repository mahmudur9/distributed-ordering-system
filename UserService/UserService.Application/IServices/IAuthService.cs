namespace UserService.Application.IServices;

public interface IAuthService
{
    string GenerateToken(Guid userId, string email, string role);
    bool VerifyPasswordHash(string password, string passwordHash);
    string GenerateHash(string password);
    Guid GetAuthenticatedUserId();
}