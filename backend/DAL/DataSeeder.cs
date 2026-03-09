using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL;

public class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
    {
        context.CreateDatabase(false);

        // Check if tables are populated
        if (context.Workspaces.Any())
        {
            return;
        }

        
        // Create Lists
        List<Workspace> workspaces =
        [
            new Workspace
            {
                Id = 1,
                Name = "Gemeente"
            },
            new Workspace
            {
                Id = 2,
                Name = "School"
            }
        ];
        
        
        // Add to database
        context.Workspaces.AddRange(workspaces);
        
        
        
        
        context.SaveChanges();
        
    }
}