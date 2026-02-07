using ProductService.Domain.Models;

namespace ProductService.Domain.IRepositories;

public interface IProductRepository : IRepository<Product>
{
}