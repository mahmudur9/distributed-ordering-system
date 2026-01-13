namespace PaymentService.Application.Requests;

public class GetAllPaymentMethodsFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}