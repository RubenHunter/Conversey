using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Identity;

public class Youth
{
    [Key]
    [Required]
    public string Token { get; set; }

    public Project.Project Project { get; set; }
    
    public string? Email { get; set; }
}