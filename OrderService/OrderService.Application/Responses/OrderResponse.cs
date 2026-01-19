namespace OrderService.Application.Responses;

public class OrderResponse : ResponseBase
{
    public decimal Amount { get; set; }
    public IEnumerable<ProductOrderResponse> Products { get; set; } = [];
}