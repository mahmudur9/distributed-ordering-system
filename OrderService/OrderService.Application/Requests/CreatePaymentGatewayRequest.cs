namespace OrderService.Application.Requests;

public class CreatePaymentGatewayRequest
{
    public decimal Amount { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentTypeId { get; set; }
    public Guid PaymentMethodId { get; set; }
}