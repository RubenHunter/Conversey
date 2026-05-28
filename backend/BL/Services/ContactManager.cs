using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Services;

public class ContactManager : IContactManager
{
    private readonly IProjectRepository _projectRepository;

    public ContactManager(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public IEnumerable<ContactEntry> GetContactsByWorkspaceId(Slug workspaceId, Slug? projectId = null, Guid? youthId = null)
    {
        var youths = _projectRepository.ReadYouthsWithRealEmailsByWorkspaceId(workspaceId, projectId);

        if (youthId.HasValue)
        {
            youths = youths.Where(y => y.Id == youthId.Value).ToList().AsReadOnly();
        }

        return youths.Select(y => new ContactEntry
        {
            YouthId = y.Id,
            Email = y.Email!,
            ProjectName = y.Project?.Name ?? string.Empty,
            ProjectId = y.Project?.Id ?? default,
            Ideas = y.Ideas.Select(i => new ContactIdea
            {
                Id = i.Id,
                Content = i.Content,
                Summary = i.Summary,
                SubmissionDate = i.SubmissionDate,
                TopicName = i.Topic?.Name ?? string.Empty
            })
        });
    }
}
