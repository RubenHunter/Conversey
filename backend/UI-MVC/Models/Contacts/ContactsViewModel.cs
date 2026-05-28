using Conversey.BL.Services;

namespace Conversey.UI_MVC.Models.Contacts;

public class ContactsViewModel
{
    public IEnumerable<ContactEntry> Contacts { get; set; } = Array.Empty<ContactEntry>();
    public string? SelectedProjectId { get; set; }
    public string? SelectedYouthId { get; set; }
    public IEnumerable<YouthFilterOption> YouthFilters { get; set; } = Array.Empty<YouthFilterOption>();
}

public class YouthFilterOption
{
    public Guid YouthId { get; set; }
    public string Email { get; set; } = string.Empty;
}
