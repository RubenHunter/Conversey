using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.UI_MVC.Models.Admin;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectViewModel
{
    public AdminFormViewModel<Project> AdminFormViewModel { get; set; } = null!;
    public StepperViewModel StepperViewModel { get; set; } = null!;
    public CreateProjectIntroAndPresentationViewModel CreateStep1ViewModel { get; set; } = new();
    public CreateStep2SurveyViewModel CreateStep2ViewModel { get; set; } = new();
    public CreateStep3IdeationViewModel CreateStep3ViewModel { get; set; } = new();
    public CreateStep4AiConfigViewModel CreateStep4ViewModel { get; set; } = new();
    public IReadOnlyList<AiPrompt> Step4Prompts { get; set; } = Array.Empty<AiPrompt>();
    public int ParticipantCount { get; set; }
    public int IdeaCount { get; set; }
    public int QuestionCount { get; set; }
    public int TopicCount { get; set; }
}
