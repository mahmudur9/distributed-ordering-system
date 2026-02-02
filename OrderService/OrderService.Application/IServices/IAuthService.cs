namespace OrderService.Application.IServices;

public interface IAuthService
{
    Guid GetAuthenticatedUserId();
    string GetAuthenticatedUserRole();
}