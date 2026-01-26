using AutoMapper;
using Microsoft.AspNetCore.Http;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Constants;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _authService = authService;
    }

    public async Task<PaginatedResponse<UserResponse>> GetAllUsersAsync(GetAllUsersFilter filter)
    {
        try
        {
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
            throw ex;
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
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(userRequest.RoleId);
            if (role is null)
            {
                throw new KeyNotFoundException("Role not found");
            }
            userRequest.Password = _authService.GenerateHash(userRequest.Password);
            await _unitOfWork.UserRepository.CreateAsync(_mapper.Map<User>(userRequest));
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdateUserAsync(Guid id, UpdateUserRequest updateUserRequest)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            if (id != _authService.GetAuthenticatedUserId() && _authService.GetAuthenticatedUserRole() != Constants.AdminRole)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            
            user = _mapper.Map(updateUserRequest, user);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            if (_authService.GetAuthenticatedUserRole() != Constants.AdminRole)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdatePasswordAsync(Guid id, UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            if (id != _authService.GetAuthenticatedUserId())
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found");
            }
            
            if (!_authService.VerifyPasswordHash(updatePasswordRequest.CurrentPassword, user.Password))
            {
                throw new ArgumentException("Incorrect current password!");
            }
            
            user.Password = _authService.GenerateHash(updatePasswordRequest.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(loginRequest.Email);
            if (user is null)
            {
                throw new ArgumentException("Incorrect email or password!");
            }
            if (!_authService.VerifyPasswordHash(loginRequest.Password, user.Password))
            {
                throw new ArgumentException("Incorrect email or password!");
            }

            var resposne = new LoginResponse()
            {
                Token = _authService.GenerateToken(user.Id, user.Email!, user.Role!.Name)
            };
            return resposne;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<UserResponse> AuthenticateAsync()
    {
        try
        {
            Guid userId = _authService.GetAuthenticatedUserId();
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);
            return _mapper.Map<UserResponse>(user);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}