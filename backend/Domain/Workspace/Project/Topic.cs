using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project;

public class Topic
{
    [Required] public int Id { get; set; }
    public Project Project { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
}