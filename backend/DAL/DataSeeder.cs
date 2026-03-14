using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
    {
        context.CreateDatabase(false);


        #region SeedWorkspaces

        var gemeente = new Workspace
        {
            Name = "Gemeente"
        };
        gemeente.Slug = Slug.FromName(gemeente.Name);


        var school = new Workspace
        {
            Name = "School",
        };
        school.Slug = Slug.FromName(school.Name);
        
        context.Workspaces.Add(gemeente);
        context.Workspaces.Add(school);

        #endregion
        
        context.SaveChanges();
        
    }
    
}