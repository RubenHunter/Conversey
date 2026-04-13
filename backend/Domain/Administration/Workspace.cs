using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Administration;

public class Workspace
{
    [Required]
    public Slug Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public IEnumerable<Project> Projects { get; set; }
}