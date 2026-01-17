using Microsoft.EntityFrameworkCore;
using UserService.Domain.IRepositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<User> SearchByName(IQueryable<User> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<List<User>> GetAllUsersAsync(string? name, bool isActive, int itemsPerPage, int pageNumber)
    {
        var users = _context.Users.Where(x => x.IsActive == isActive).Include(x => x.Role).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            users = SearchByName(users, name);
        }

        users = users.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await users.ToListAsync();
    }

    public async Task<int> GetAllUserCountAsync(string? name, bool isActive)
    {
        var users = _context.Users.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            users = SearchByName(users, name);
        }

        return await users.CountAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.Include(x => x.Role).AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
        return user!;
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.Include(x => x.Role).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return user!;
    }
}