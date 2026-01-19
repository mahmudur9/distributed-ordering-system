namespace OrderService.Application.Requests;

public class GetAllOrderFilter : PaginationBase
{
    public bool IsActive { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}