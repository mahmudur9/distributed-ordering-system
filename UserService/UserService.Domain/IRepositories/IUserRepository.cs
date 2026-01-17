using UserService.Domain.Models;

namespace UserService.Domain.IRepositories;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> GetAllUsersAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllUserCountAsync(string? name, bool isActive);
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(Guid id);
}