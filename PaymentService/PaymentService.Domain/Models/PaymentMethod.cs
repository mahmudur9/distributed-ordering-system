using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Domain.Models;

public class PaymentMethod : Base
{
    public required string Name { get; set; }
    public required Guid PaymentTypeId { get; set; }
    [ForeignKey(nameof(PaymentTypeId))]
    public virtual PaymentType? PaymentType { get; set; }
}