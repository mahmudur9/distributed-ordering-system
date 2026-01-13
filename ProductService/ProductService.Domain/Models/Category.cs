namespace ProductService.Domain.Models;

public class Category : Base
{
    public required string Name { get; set; }
    public virtual List<Product> Products { get; set; } = [];
}