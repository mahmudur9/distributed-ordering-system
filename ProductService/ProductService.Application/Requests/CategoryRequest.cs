using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests;

public class CategoryRequest
{
    [Required(ErrorMessage =  "Name is required")]
    public string Name { get; set; }
}