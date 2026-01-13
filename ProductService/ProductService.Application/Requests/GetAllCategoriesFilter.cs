namespace ProductService.Application.Requests;

public class GetAllCategoriesFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}