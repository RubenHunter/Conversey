using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Conversey.BL.Domain.Entities.Identity;

public class Workspace
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Slug { get; set; }
    
    [Required]
    public Conversey Conversey { get; set; }

    public IEnumerable<Project.Project> Projects { get; set; }
    
    public Workspace(string name, Func<string, bool> slugExists)
    {
        Name = name;

        Slug = GenerateSlug(name);

        if (slugExists(Slug))
        {
            throw new InvalidOperationException("Workspace already exists");
        }
    }

    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workspace name cannot be empty");
        }

        var slug = name.Trim().ToLower().Replace(" ", "_");
        
        //Remove url unfriendly symbols
        slug = Regex.Replace(slug, @"[^a-z0-9_]", "");
        
        return slug;
    }
}