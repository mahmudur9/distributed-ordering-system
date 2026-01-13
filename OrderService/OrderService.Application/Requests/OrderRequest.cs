namespace OrderService.Application.Requests;

public class OrderRequest
{
    public IEnumerable<ProductOrderRequest> Products { get; set; } = [];
    public required PaymentRequest Payment { get; set; }
}