using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey;
using System.Text.RegularExpressions;

namespace Conversey.BL.Domain.Subplatform;

public class Workspace
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public IEnumerable<Project> Projects { get; set; }
    
    
    [Required]
    public string Slug { get; set; }
    
    // [Required]
    // public Conversey Conversey { get; set; }
    
    public Workspace(string name, string slug,Func<string, bool> slugExists)
    {
        Name = name;
        Slug = slug;

        if (slugExists(Slug))
        {
            throw new InvalidOperationException("Workspace already exists");
        }
        
        Projects = new List<Project>();
    }
    
    // Parameterless constructor for EF
    protected Workspace()
    {
        Name = string.Empty;
        Slug = string.Empty;
        Projects = new List<Project>();
    }

    
}