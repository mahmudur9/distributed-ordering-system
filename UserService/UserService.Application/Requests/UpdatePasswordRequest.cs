using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests;

public class UpdatePasswordRequest
{
    [Required(ErrorMessage =  "Current password is required")]
    public string CurrentPassword { get; set; }
    [Required(ErrorMessage =  "New password is required")]
    public string NewPassword { get; set; }
}