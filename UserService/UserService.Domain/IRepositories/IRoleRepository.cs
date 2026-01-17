using UserService.Domain.Models;

namespace UserService.Domain.IRepositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<List<Role>> GetAllRolesAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllRoleCountAsync(string? name, bool isActive);
}