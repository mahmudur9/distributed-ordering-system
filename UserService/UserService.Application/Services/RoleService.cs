using AutoMapper;
using UserService.Application.IServices;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.IRepositories;

namespace UserService.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<RoleResponse>> GetAllRolesAsync(GetAllRolesFilter filter)
    {
        try
        {
            var roles = await _unitOfWork.RoleRepository.GetAllRolesAsync(filter.Name, 
                filter.IsActive, filter.ItemsPerPage, filter.PageNumber);
            var roleCount = await _unitOfWork.RoleRepository.GetAllRoleCountAsync(filter.Name, filter.IsActive);

            var paginatedResponse = new PaginatedResponse<RoleResponse>(
                _mapper.Map<IEnumerable<RoleResponse>>(roles),
                roleCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}