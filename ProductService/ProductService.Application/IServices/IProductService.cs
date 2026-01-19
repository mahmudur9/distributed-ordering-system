using ProductService.Application.Requests;
using ProductService.Application.Responses;

namespace ProductService.Application.IServices;

public interface IProductService
{
    Task<PaginatedResponse<ProductResponse>> GetAllProductsAsync(GetAllProductsFilter filter);
    Task<ProductResponse> GetProductByIdAsync(Guid id);
    Task CreateProductAsync(ProductRequest productRequest);
    Task UpdateProductAsync(Guid id, ProductRequest productRequest);
    Task DeleteProductAsync(Guid id);
}