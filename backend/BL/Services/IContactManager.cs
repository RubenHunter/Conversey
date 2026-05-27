using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Services;

public interface IContactManager
{
    IEnumerable<ContactEntry> GetContactsByWorkspaceId(Slug workspaceId, Slug? projectId = null, Guid? youthId = null);
}

public class ContactEntry
{
    public Guid YouthId { get; set; }
    public string Email { get; set; }
    public string ProjectName { get; set; }
    public Slug ProjectId { get; set; }
    public IEnumerable<ContactIdea> Ideas { get; set; } = Array.Empty<ContactIdea>();
}

public class ContactIdea
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string TopicName { get; set; }
}

public class SendContactEmailRequest
{
    public Guid? YouthId { get; set; }
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
