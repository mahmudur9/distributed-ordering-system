namespace UserService.Application.Responses;

public class UserResponse
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; }
    public string RoleName { get; set; }
}