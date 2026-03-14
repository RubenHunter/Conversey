using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey;
using System.Text.RegularExpressions;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Subplatform;

public class Workspace
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public IEnumerable<Project> Projects { get; set; } = new List<Project>();
    
    
    [Required]
    public Slug Slug { get; set; }
    
    // [Required]
    // public Conversey Conversey { get; set; }
}