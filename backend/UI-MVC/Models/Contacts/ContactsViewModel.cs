using Conversey.BL.Services;

namespace Conversey.UI_MVC.Models.Contacts;

public class ContactsViewModel
{
    public IEnumerable<ContactEntry> Contacts { get; set; } = Array.Empty<ContactEntry>();
    public string? SelectedProjectId { get; set; }
}
