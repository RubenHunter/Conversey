using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Models.Dto;

public class WorkspaceDto
{
    public Slug Id { get; set; }
    public string Name { get; set; }
    

    public static WorkspaceDto From(Workspace workspace)
    {
        return new WorkspaceDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
        };
    }
}