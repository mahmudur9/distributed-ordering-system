namespace PaymentService.Domain.Models;

public class PaymentType : Base
{
    public required string Name { get; set; }
    public virtual List<PaymentMethod> PaymentMethods { get; set; } = [];
}