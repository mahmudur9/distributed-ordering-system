using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Models;

public class Picture : Base
{
    public string Url { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}