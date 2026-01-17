using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests;

public class UpdateUserRequest
{
    [Required(ErrorMessage =  "User name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage =  "Email is required")]
    public string Email { get; set; }
    public string? Phone { get; set; }
    [Required(ErrorMessage =  "Role is required")]
    public Guid RoleId { get; set; }
}