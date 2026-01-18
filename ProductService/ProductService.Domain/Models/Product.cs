using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Models;

public class Product : Base
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal BuyPrice { get; set; }
    public required decimal SellingPrice { get; set; }
    public required int Stock { get; set; }
    public required Guid CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }
}