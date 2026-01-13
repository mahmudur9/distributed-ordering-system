using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests;

public class ProductRequest
{
    [Required(ErrorMessage =  "Name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage =  "Price is required")]
    public decimal Price { get; set; }
    [Required(ErrorMessage =  "CategoryId is required")]
    public Guid CategoryId { get; set; }
}