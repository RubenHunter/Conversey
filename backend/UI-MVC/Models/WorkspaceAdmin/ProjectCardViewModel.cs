using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Administration;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class ProjectCardViewModel
{
    public Slug Id { get; set; }
    public string Title { get; set; }
    public string ImageUrl { get; set; }
    public Status Status { get; set; }
}
