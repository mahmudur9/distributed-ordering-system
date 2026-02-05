namespace OrderService.Application.Requests;

public class ProductStockGatewayRequest
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}