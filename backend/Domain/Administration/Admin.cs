using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Administration;

public abstract class Admin
{
    [Required] 
    public Guid Id { get; set; }
    
    [Required] 
    public string Email { get; set; }
}

public class ConverseyAdmin : Admin
{

}

public class WorkspaceAdmin : Admin
{
    [Required]
    public Workspace Workspace { get; set; }
}