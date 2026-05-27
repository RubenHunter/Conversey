namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectsPageViewModel
{
    public List<ProjectCardViewModel> Projects { get; set; } = [];
    public int TotalProjects { get; set; }
    public int ParticipantCount { get; set; }
    public int IdeaCount { get; set; }
    public int AnswerCount { get; set; }
}
