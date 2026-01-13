namespace ProductService.Application.Responses;

public class ProductResponse : ResponseBase
{
    public string? Name { get; set; }
    public decimal price { get; set; }
    public virtual CategoryResponse? Category { get; set; }
}