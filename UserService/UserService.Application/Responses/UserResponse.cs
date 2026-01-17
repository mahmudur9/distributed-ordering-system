namespace UserService.Application.Responses;

public class UserResponse : ResponseBase
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string RoleName { get; set; }
}