using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.Models;

namespace ProductService.Application.IServices;

public interface ICategoryService
{
    Task<PaginatedResponse<CategoryResponse>> GetAllCategoriesAsync(GetAllCategoriesFilter getAllCategoriesFilter);
    Task<CategoryResponse> GetCategoryByIdAsync(Guid id);
    Task CreateCategoryAsync(CategoryRequest categoryRequest);
    Task UpdateCategoryAsync(Guid id, CategoryRequest categoryRequest);
    Task DeleteCategoryAsync(Guid id);
}