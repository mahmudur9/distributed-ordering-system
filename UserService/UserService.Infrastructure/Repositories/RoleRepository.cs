using Microsoft.EntityFrameworkCore;
using UserService.Application.Constants;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<Role> SearchByName(IQueryable<Role> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<List<Role>> GetAllRolesAsync(string? name, bool isActive, int itemsPerPage, int pageNumber)
    {
        var roles = _context.Roles.Where(x => x.IsActive == isActive && 
                                              x.Name != ApplicationConstants.AdminRole).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            roles = SearchByName(roles, name);
        }

        roles = roles.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await roles.ToListAsync();
    }

    public async Task<int> GetAllRoleCountAsync(string? name, bool isActive)
    {
        var roles = _context.Roles.Where(x => x.IsActive == isActive && 
                                              x.Name != ApplicationConstants.AdminRole).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            roles = SearchByName(roles, name);
        }

        return await roles.CountAsync();
    }
}