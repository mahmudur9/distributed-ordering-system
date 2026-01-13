using Microsoft.EntityFrameworkCore;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DBContext context) : base(context)
    {
    }
    
    private IQueryable<Product> SearchByName(IQueryable<Product> query, string name)
    {
        return query.Where(x => (x.Name).ToLower().Contains(name.ToLower()));
    }

    public async Task<List<Product>> GetAllProductsAsync(string? name, bool isActive, int itemsPerPage, int pageNumber)
    {
        var products = _context.Products.Where(x => x.IsActive == isActive).Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            products = SearchByName(products, name);
        }

        products = products.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

        return await products.ToListAsync();
    }

    public async Task<int> GetAllProductCountAsync(string? name, bool isActive)
    {
        var products = _context.Products.Where(x => x.IsActive == isActive).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            products = SearchByName(products, name);
        }

        return await products.CountAsync();
    }
}