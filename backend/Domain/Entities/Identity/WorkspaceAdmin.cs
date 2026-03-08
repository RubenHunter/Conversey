using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Identity;

public class WorkspaceAdmin :  Admin
{
    [Required]
    public int Id { get; set; }
    
    public Workspace Workspace { get; set; }
}