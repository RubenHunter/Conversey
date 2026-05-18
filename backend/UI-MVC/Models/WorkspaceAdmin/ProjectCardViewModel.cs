using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectCardViewModel
{
    public Slug Id { get; set; }
    public string Title { get; set; }
    public string ImageUrl { get; set; }
}