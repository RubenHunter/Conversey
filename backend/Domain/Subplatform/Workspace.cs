using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.BL.Domain.Subplatform;

public class Workspace
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public IEnumerable<Project> Projects { get; set; }
    
    
}