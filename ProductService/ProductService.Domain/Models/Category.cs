using System.Text.Json.Serialization;

namespace ProductService.Domain.Models;

public class Category : Base
{
    public required string Name { get; set; }
    [JsonIgnore]
    public virtual List<Product> Products { get; set; } = [];
}