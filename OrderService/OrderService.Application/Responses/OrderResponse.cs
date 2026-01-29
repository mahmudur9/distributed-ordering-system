namespace OrderService.Application.Responses;

public class OrderResponse : ResponseBase
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public IEnumerable<ProductOrderResponse> Products { get; set; } = [];
}