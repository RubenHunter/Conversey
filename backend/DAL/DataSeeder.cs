using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL;

public class DataSeeder
{
    private static ConverseyDbContext _context;
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
        var slugExists = new Func<Slug, bool>(slug => _context!.Workspaces.Any(w => w.Slug == slug));
        var workspace = new Workspace
        {
            Name = name,
            Slug = Slug.FromName(name)
        };
        if (slugExists(workspace.Slug)) throw new ValidationException($"Workspace Slug '{workspace.Slug.Text}' already exists.");
        
        Validate(workspace);
        _context.Workspaces.Add(workspace);
        return workspace;;
    }
    
    
    private static void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            throw new ValidationException(string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }
    }
}