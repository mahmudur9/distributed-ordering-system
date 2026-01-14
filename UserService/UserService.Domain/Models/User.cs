using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Models;

public class User : Base
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }
    public required Guid RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }
}