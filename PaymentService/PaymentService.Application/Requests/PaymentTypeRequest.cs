using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Requests;

public class PaymentTypeRequest
{
    [Required(ErrorMessage = "Payment type name is required")]
    public string Name { get; set; }
}