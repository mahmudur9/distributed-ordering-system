using Microsoft.AspNetCore.Mvc;
using ProductService.Application.IServices;
using ProductService.Application.Requests;

namespace ProductService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllCategoriesFilter filter)
    {
        return Ok(await _categoryService.GetAllCategoriesAsync(filter));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _categoryService.GetCategoryByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest categoryRequest)
    {
        await _categoryService.CreateCategoryAsync(categoryRequest);
        return Ok("Category created.");
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CategoryRequest categoryRequest)
    {
        await _categoryService.UpdateCategoryAsync(id, categoryRequest);
        return Ok("Category updated.");
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return Ok("Category deleted.");
    }
}