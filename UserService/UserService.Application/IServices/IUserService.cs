using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Application.IServices;

public interface IUserService
{
    Task<PaginatedResponse<UserResponse>> GetAllUsersAsync(GetAllUsersFilter filter);
    Task<UserResponse> GetUserByIdAsync(Guid id);
    Task CreateUserAsync(UserRequest userRequest);
    Task UpdateUserAsync(Guid id, UpdateUserRequest updateUserRequest);
    Task DeleteUserAsync(Guid id);
    Task UpdatePasswordAsync(Guid id, UpdatePasswordRequest updatePasswordRequest);
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<UserResponse> AuthenticateAsync();
}