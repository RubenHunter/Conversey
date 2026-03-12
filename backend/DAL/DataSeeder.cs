using System.Text.RegularExpressions;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL;

public class DataSeeder
{
    private static ConverseyDbContext? _context;
    public static void Seed(ConverseyDbContext context)
    {
        _context = context;
        _context.CreateDatabase(false);
        
        
        // Create Lists
        List<Workspace> workspaces =
        [
            CreateWorkspace
            (
                "Gemeente"
            ),
            CreateWorkspace
            (
                "School"
            ),
            CreateWorkspace
            (
                "Axa Bank"
            )
        ];
        
        
        // Add to database
        _context.Workspaces.AddRange(workspaces);
        
        
        
        
        _context.SaveChanges();
        
    }

    private static Workspace CreateWorkspace(string name)
    {
        var slugExists = new Func<string, bool>(slug => _context!.Workspaces.Any(w => w.Slug == slug));
        return new Workspace(name, GenerateSlug(name), slugExists);
    }
    
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workspace name cannot be empty");
        }

        var slug = name.Trim().ToLower().Replace(" ", "-");
        
        //Remove url unfriendly symbols
        slug = Regex.Replace(slug, @"[^a-z0-9_-]", "");
        
        return slug;
    }
}