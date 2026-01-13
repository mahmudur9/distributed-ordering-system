namespace PaymentService.Application.Requests;

public class PaginationBase
{
    public int PageNumber { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 2;
}