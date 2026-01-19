namespace OrderService.Application.Requests;

public class OrderRequest
{
    public required Guid PaymentTypeId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public IEnumerable<ProductOrderRequest> Products { get; set; } = [];
}