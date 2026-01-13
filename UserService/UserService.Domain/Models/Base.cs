using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Models;

public class Base
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}