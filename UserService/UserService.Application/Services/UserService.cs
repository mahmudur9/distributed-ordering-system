using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Constants;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authService = authService;
        _logger = logger;
    }

    public async Task<PaginatedResponse<UserResponse>> GetAllUsersAsync(GetAllUsersFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all users");
            var users = await _unitOfWork.UserRepository.GetAllUsersAsync(filter.Name,
                filter.IsActive, filter.ItemsPerPage, filter.PageNumber);
            var userCount = await _unitOfWork.UserRepository.GetAllUserCountAsync(filter.Name, filter.IsActive);

            var paginatedResponse = new PaginatedResponse<UserResponse>(
                _mapper.Map<IEnumerable<UserResponse>>(users),
                userCount,
                filter.ItemsPerPage,
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            throw;
        }
    }

    public Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task CreateUserAsync(UserRequest userRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new user");
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(userRequest.RoleId);
            if (role is null) throw new KeyNotFoundException("Role not found");
            userRequest.Password = _authService.GenerateHash(userRequest.Password);
            await _unitOfWork.UserRepository.CreateAsync(_mapper.Map<User>(userRequest));
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            throw;
        }
    }

    public async Task UpdateUserAsync(Guid id, UpdateUserRequest updateUserRequest)
    {
        try
        {
            _logger.LogInformation($"Updating a user with id {id}");
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null) throw new Exception("User not found");

            if (id != _authService.GetAuthenticatedUserId() &&
                _authService.GetAuthenticatedUserRole() != Constants.AdminRole)
                throw new UnauthorizedAccessException("Unauthorized");

            user = _mapper.Map(updateUserRequest, user);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update user with id {id}");
            throw;
        }
    }

    public async Task DeleteUserAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Deleting a user with id {id}");
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null) throw new Exception("User not found");

            if (_authService.GetAuthenticatedUserRole() != Constants.AdminRole)
                throw new UnauthorizedAccessException("Unauthorized");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete user with id {id}");
            throw;
        }
    }

    public async Task UpdatePasswordAsync(Guid id, UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            _logger.LogInformation($"Updating a user password with id {id}");
            if (id != _authService.GetAuthenticatedUserId()) throw new UnauthorizedAccessException("Unauthorized");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null) throw new KeyNotFoundException("User not found");

            if (!_authService.VerifyPasswordHash(updatePasswordRequest.CurrentPassword, user.Password))
                throw new ArgumentException("Incorrect current password!");

            user.Password = _authService.GenerateHash(updatePasswordRequest.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update user password with id {id}");
            throw;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            _logger.LogInformation($"Login a user with email {loginRequest.Email}");
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(loginRequest.Email);
            if (user is null) throw new ArgumentException("Incorrect email or password!");
            if (!_authService.VerifyPasswordHash(loginRequest.Password, user.Password))
                throw new ArgumentException("Incorrect email or password!");

            var resposne = new LoginResponse
            {
                Token = _authService.GenerateToken(user.Id, user.Email!, user.Role!.Name)
            };
            return resposne;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed login with email {loginRequest.Email}");
            throw;
        }
    }

    public async Task<UserResponse> AuthenticateAsync()
    {
        try
        {
            var userId = _authService.GetAuthenticatedUserId();
            _logger.LogInformation($"Authenticating a user with id {userId}");
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
            return _mapper.Map<UserResponse>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to authenticate a user with id {_authService.GetAuthenticatedUserId()}");
            throw;
        }
    }
}