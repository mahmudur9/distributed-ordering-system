namespace UserService.Application.Requests;

public class GetAllUsersFilter : PaginationBase
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
}