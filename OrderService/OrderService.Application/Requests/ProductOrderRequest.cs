namespace OrderService.Application.Requests;

public class ProductOrderRequest
{
    public string ProductName { get; set; }
    public Guid ProductId { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}