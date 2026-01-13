namespace OrderService.Application.Requests;

public class GetAllOrderFilter
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}