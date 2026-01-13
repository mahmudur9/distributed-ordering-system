namespace PaymentService.Application.Responses;

public class PaymentTypeResponse : ResponseBase
{
    public string Name { get; set; }
    public IEnumerable<PaymentMethodResponse> PaymentMethods { get; set; } = [];
}