using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Administration;

public abstract class Admin
{
    [Required] 
    public Guid Id { get; set; }
    
    [Required] 
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Username { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    [Required] 
    public bool FirstLogin { get; set; } = false;
}

public class ConverseyAdmin : Admin
{

}

public class WorkspaceAdmin : Admin
{
    [Required]
    public Workspace Workspace { get; set; }
}
