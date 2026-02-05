namespace UserService.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string GenerateHash(string password);
    bool VerifyPasswordHash(string password, string passwordHash);
}