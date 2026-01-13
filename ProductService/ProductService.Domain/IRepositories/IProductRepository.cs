using ProductService.Domain.Models;

namespace ProductService.Domain.IRepositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetAllProductsAsync(string? name, bool isActive, int itemsPerPage, int pageNumber);
    Task<int> GetAllProductCountAsync(string? name, bool isActive);
}