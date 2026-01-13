namespace ProductService.Application.Requests;

public class GetAllProductsFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}