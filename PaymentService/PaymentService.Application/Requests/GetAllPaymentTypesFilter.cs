namespace PaymentService.Application.Requests;

public class GetAllPaymentTypesFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}