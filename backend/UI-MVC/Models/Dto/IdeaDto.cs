using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.UI_MVC.Models.Dto;

public class IdeaDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public Slug ProjectId { get; set; }
    public int TopicId { get; set; }
    public Guid YouthId { get; set; }
    public DateTime SubmissionDate { get; set; }
    public ModerationStatus Status { get; set; }
    public IEnumerable<ReactionDto> Reactions { get; set; }

    public static IdeaDto From(Idea idea)
    {
        return new IdeaDto
        {
            Id = idea.Id,
            Content = idea.Content,
            ProjectId = idea.Project.Id,
            TopicId = idea.Topic.Id,
            YouthId = idea.Youth.Id,
            SubmissionDate = idea.SubmissionDate,
            Status = idea.Status,
            Reactions = (idea.Reactions ?? Array.Empty<IdeaReaction>())
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto { Emoji = g.Key, Count = g.Count() })
        };
    }
}
