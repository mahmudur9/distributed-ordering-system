using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Domain.Models;

public class Payment : Base
{
    public decimal Amount { get; set; }
    public required Guid OrderId { get; set; }
    public required Guid PaymentTypeId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    [ForeignKey(nameof(PaymentTypeId))]
    public PaymentType PaymentType { get; set; }
    [ForeignKey(nameof(PaymentMethodId))]
    public PaymentMethod PaymentMethod { get; set; }
}