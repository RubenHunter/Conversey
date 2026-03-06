using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.REST.Models.Dto;

public class WorkspaceDto
{
    public string Name { get; set; }


    public static WorkspaceDto From(Workspace workspace)
    {
        return new WorkspaceDto
        {
            Name = workspace.Name,
        };
    }
}