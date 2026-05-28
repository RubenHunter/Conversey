using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Survey;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = null!;
    public IEnumerable<Question> Questions { get; set; } = [];
    public int ParticipantCount { get; set; }
    public int IdeaCount { get; set; }
}
