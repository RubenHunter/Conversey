using Conversey.BL.Domain.Administration;

namespace Conversey.UI_MVC.Models.Admin;

public class ProjectFormViewModel
{
    public Project Project { get; set; }
    public string FormAction { get; set; }
    public string SubmitLabel { get; set; }
    public bool IsEdit { get; set; }
}