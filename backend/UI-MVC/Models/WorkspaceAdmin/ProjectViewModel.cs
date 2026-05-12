using Conversey.BL.Domain.Administration;
using Conversey.UI_MVC.Models.Admin;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectViewModel
{
    public AdminFormViewModel<Project> AdminFormViewModel { get; set; }
    public StepperViewModel StepperViewModel { get; set; }
}