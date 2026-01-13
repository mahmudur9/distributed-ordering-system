using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Models;

public class Product : Base
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required Guid CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }
}