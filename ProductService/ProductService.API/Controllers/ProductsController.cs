using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.IServices;
using ProductService.Application.Requests;

namespace ProductService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "User")]
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductsFilter filter)
    {
        var products = await _productService.GetAllProductsAsync(filter);
        return Ok(products);
    }

    // [Authorize(Roles = "Admin")]
    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] ProductRequest productRequest)
    {
        await _productService.CreateProductAsync(productRequest);
        return Ok("Product created.");
    }
    
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] ProductUpdateRequest productRequest)
    {
        await _productService.UpdateProductAsync(id, productRequest);
        return Ok("Product updated.");
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteProductAsync(id);
        return Ok("Product deleted.");
    }
}