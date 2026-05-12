namespace Conversey.UI_MVC.Models.Admin;

public class StepperViewModel
{
    public string Title { get; set; } = string.Empty; // example: "Creating a project"
    public string EntityName { get; set; } = string.Empty; // example: "Project"
    public string DraftStoragePrefix { get; set; } = string.Empty;
    public string ImageUploadUrl { get; set; } = string.Empty;
    public List<StepItem> Steps { get; set; } = [];
}

public class StepItem
{
    public string Label { get; set; } = string.Empty;
    public string PartialViewName { get; set; } = string.Empty;
}
