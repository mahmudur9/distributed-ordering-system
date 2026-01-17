namespace UserService.Application.Requests;

public class GetAllRolesFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}