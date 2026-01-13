namespace OrderService.Application.Requests;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public required Guid PaymentTypeId { get; set; }
    public Guid? PaymentMethodId { get; set; }
}