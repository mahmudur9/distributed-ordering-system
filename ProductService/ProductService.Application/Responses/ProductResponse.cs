namespace ProductService.Application.Responses;

public class ProductResponse : ResponseBase
{
    public string? Name { get; set; }
    public string Description { get; set; }
    public decimal SellingPrice { get; set; }
    public int Stock { get; set; }
    public virtual CategoryResponse? Category { get; set; }
}