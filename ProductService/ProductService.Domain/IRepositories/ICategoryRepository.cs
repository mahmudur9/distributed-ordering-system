using ProductService.Domain.Models;

namespace ProductService.Domain.IRepositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllCategoriesAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllCategoryCountAsync(string? name, bool isActive);
}