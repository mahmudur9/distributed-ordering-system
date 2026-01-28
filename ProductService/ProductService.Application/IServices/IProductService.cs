using ProductService.Application.Requests;
using ProductService.Application.Responses;

namespace ProductService.Application.IServices;

public interface IProductService
{
    Task<PaginatedResponse<ProductResponse>> GetAllProductsAsync(GetAllProductsFilter filter);
    Task<PaginatedResponse<ProductResponse>> GetAllProductsFromRedisAsync(GetAllProductsFilter filter);
    Task<ProductResponse> GetProductByIdAsync(Guid id);
    Task CreateProductAsync(ProductRequest productRequest);
    Task UpdateProductAsync(Guid id, ProductUpdateRequest productRequest);
    Task DeleteProductAsync(Guid id);
    Task VerifyAndUpdateProductStockAsync(List<UpdateProductStockRequest> updateProductStockRequest);
}