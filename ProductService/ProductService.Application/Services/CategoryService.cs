using System.Linq.Expressions;
using AutoMapper;
using ProductService.Application.IServices;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.ILogging;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;

namespace ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAppLogger<CategoryService> _logger;
    
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<CategoryResponse>> GetAllCategoriesAsync(GetAllCategoriesFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all categories from database");
            List<Expression<Func<Category, bool>>> filters = [];
            filters.Add(x => x.IsActive == filter.IsActive);
            if (filter.Name is not null) filters.Add(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(filters, filter.ItemsPerPage, filter.PageNumber, 
                x => x.OrderByDescending(o => o.CreatedAt));
            var productCount = await _unitOfWork.CategoryRepository.CountAsync(filters);

            var paginatedResponse = new PaginatedResponse<CategoryResponse>(
                _mapper.Map<IEnumerable<CategoryResponse>>(categories),
                productCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all categories from database");
            throw;
        }
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting category with id {id}");
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                throw new KeyNotFoundException($"Category with id {id} not found");
            }
            return _mapper.Map<CategoryResponse>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get category with id {id}");
            throw;
        }
    }

    public async Task CreateCategoryAsync(CategoryRequest categoryRequest)
    {
        try
        {
            _logger.LogInformation("Creating new category");
            var category = _mapper.Map<Category>(categoryRequest);
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            category.IsActive = true;
            await _unitOfWork.CategoryRepository.CreateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category");
            throw;
        }
    }

    public async Task UpdateCategoryAsync(Guid id, CategoryRequest categoryRequest)
    {
        try
        {
            _logger.LogInformation("Updating category");
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                throw new KeyNotFoundException($"Category with id {id} not found");
            }
            
            category =  _mapper.Map(categoryRequest, category);
            category.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CategoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update category");
            throw;
        }
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting category");
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                throw new KeyNotFoundException($"Category with id {id} not found");
            }

            List<Expression<Func<Category, bool>>> filters = [];
            filters.Add(x => x.Id == id && x.Products.Any(p => p.IsActive));
            
            bool hasActiveProducts = await _unitOfWork.CategoryRepository
                .AnyAsync(filters);
            if (hasActiveProducts)
            {
                throw new ArgumentException("You can not delete a category with active products!");
            }
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CategoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete category");
            throw;
        }
    }
}