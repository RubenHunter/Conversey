using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;

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
        
        #region SeedProjects

        var openbaarVervoer = new Project
        {
            Title = "Openbaar Vervoer",
            Workspace = gemeente,
        };
        openbaarVervoer.Slug = Slug.FromName(openbaarVervoer.Title);

        var mentaal = new Project
        {
            Title = "Mentale gezondheid",
            Workspace = school,
        };
        mentaal.Slug = Slug.FromName(mentaal.Title);
        
        context.Projects.Add(openbaarVervoer);
        context.Projects.Add(mentaal);

        #endregion
        
        context.SaveChanges();
        
    }
    
}