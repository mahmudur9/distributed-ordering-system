using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Requests;

public class PaymentMethodRequest
{
    [Required(ErrorMessage = "Payment method name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Payment type id is required")]
    public Guid PaymentTypeId { get; set; }
}