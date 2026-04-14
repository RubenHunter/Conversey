using Conversey.BL.Domain.Administration;

namespace Conversey.REST.Models.Dto;

public class WorkspaceDto
{
    public string Name { get; set; }
    public string Slug { get; set; }

    public static WorkspaceDto From(Workspace workspace)
    {
        return new WorkspaceDto
        {
            Name = workspace.Name,
            Slug = workspace.Id.Text
        };
    }
}