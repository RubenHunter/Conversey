using Conversey.BL.Domain.Administration;
using Conversey.UI_MVC.Models.Admin;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectViewModel
{
    public AdminFormViewModel<Project> AdminFormViewModel { get; set; } = null!;
    public StepperViewModel StepperViewModel { get; set; } = null!;
    public CreateProjectIntroAndPresentationViewModel CreateStep1ViewModel { get; set; } = new();
    public CreateStep3IdeationViewModel CreateStep3ViewModel { get; set; } = new();
}
