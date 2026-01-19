namespace ProductService.Application.Requests;

public class UpdateProductStockRequest
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}