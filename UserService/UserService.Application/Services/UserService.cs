using AutoMapper;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.IRepositories;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

    public Task CreateUserAsync(UserRequest userRequest)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserAsync(Guid id, UserRequest userRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}