namespace Conversey.BL.Domain.Entities.Identity;

public class Conversey
{
    public IEnumerable<Workspace> Workspaces { get; set; }

    public IEnumerable<ConverseyAdmin> ConverseyAdmins { get; set; }
}