using Microsoft.EntityFrameworkCore;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<Category> SearchByName(IQueryable<Category> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<List<Category>> GetAllCategoriesAsync(string? name, bool isActive, int itemsPerPage,
        int pageNumber)
    {
        var categories = _context.Categories.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            categories = SearchByName(categories, name);
        }

        categories = categories.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await categories.ToListAsync();
    }

    public async Task<int> GetAllCategoryCountAsync(string? name, bool isActive)
    {
        var categories = _context.Categories.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            categories = SearchByName(categories, name);
        }

        return await categories.CountAsync();
    }
}