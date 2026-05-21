using System.Collections.Generic;

namespace Conversey.UI_MVC.Models.Shared;

public class AdminCardGridViewModel
{
    public IReadOnlyList<AdminCardViewModel> Cards { get; set; } = [];
    public string AddNewUrl { get; set; }
    public string AddNewLabel { get; set; }
    public string AddNewAriaLabel { get; set; }
}

public class AdminCardViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; }
    public string StatusLabel { get; set; }
    public string ImageUrl { get; set; }
    public string ViewUrl { get; set; } = string.Empty;
    public string ViewAriaLabel { get; set; }

    public AdminCardActionViewModel EditAction { get; set; } = new();
    public AdminCardActionViewModel ArchiveAction { get; set; } = new();
    public AdminCardActionViewModel CopyAction { get; set; } = new();
    public AdminCardActionViewModel NotificationsAction { get; set; } = new();
    public AdminCardActionViewModel ShareAction { get; set; } = new();
}

public class AdminCardActionViewModel
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string AriaLabel { get; set; }
    public bool Enabled { get; set; } = true;
    public Dictionary<string, string> DataAttributes { get; set; } = new();
}
