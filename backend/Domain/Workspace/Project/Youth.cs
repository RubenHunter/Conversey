using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project;

public class Youth
{
    [Key]
    [Required]
    public string Token { get; set; }

    public Project Project { get; set; }
    
    public string? Email { get; set; }
}