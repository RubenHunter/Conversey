using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Administration;

public class WorkspaceAdmin
{
    [Required]
    public int Id { get; set; }
    
    public Workspace Workspace { get; set; }
}