using AutoMapper;
using ProductService.Application.IServices;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ProductResponse>> GetAllProductsAsync(GetAllProductsFilter getAllProductsFilter)
    {
        try
        {
            var products = await _unitOfWork.ProductRepository.GetAllProductsAsync(getAllProductsFilter.Name, 
                getAllProductsFilter.IsActive, getAllProductsFilter.ItemsPerPage, getAllProductsFilter.PageNumber);
            var productCount = await _unitOfWork.ProductRepository.GetAllProductCountAsync(getAllProductsFilter.Name, getAllProductsFilter.IsActive);

            var paginatedResponse = new PaginatedResponse<ProductResponse>(
                _mapper.Map<IEnumerable<ProductResponse>>(products),
                productCount,
                getAllProductsFilter.ItemsPerPage, 
                getAllProductsFilter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            return _mapper.Map<ProductResponse>(product);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task CreateProductAsync(ProductRequest productRequest)
    {
        try
        {
            var product = _mapper.Map<Product>(productRequest);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsActive = true;
            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdateProductAsync(Guid id, ProductRequest productRequest)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            
            product =  _mapper.Map(productRequest, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task DeleteProductAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}