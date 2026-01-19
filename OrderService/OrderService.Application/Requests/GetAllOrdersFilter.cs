namespace OrderService.Application.Requests;

public class GetAllOrdersFilter : PaginationBase
{
    public bool IsActive { get; set; } = true;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}