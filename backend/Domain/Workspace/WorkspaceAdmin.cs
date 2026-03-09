using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace;

public class WorkspaceAdmin
{
    [Required]
    public int Id { get; set; }
    
    public Workspace Workspace { get; set; }
}