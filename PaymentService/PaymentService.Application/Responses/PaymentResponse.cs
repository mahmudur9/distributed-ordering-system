namespace PaymentService.Application.Responses;

public class PaymentResponse : ResponseBase
{
    public decimal Amount { get; set; }
    public required Guid OrderId { get; set; }
    public required Guid PaymentTypeId { get; set; }
    public string PaymentTypeName { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; }
}