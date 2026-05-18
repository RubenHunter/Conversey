namespace Conversey.UI_MVC.Models.Admin;

public class StepperViewModel
{
    public string Title { get; set; } // example: "Creating a project"
    public string EntityName { get; set; } // example: "Project"
    public List<StepItem> Steps { get; set; }
}

public class StepItem
{
    public string Label { get; set; }
    public string PartialViewName { get; set; }
}