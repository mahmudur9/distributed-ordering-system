namespace PaymentService.Application.Requests;

public class GetAllPaymentsFilter : PaginationBase
{
    public bool IsActive { get; set; } = true;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? PaymentId { get; set; }
}