using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace;

public class Workspace
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public IEnumerable<Project.Project> Projects { get; set; }
    
    
}