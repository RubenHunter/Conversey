using Conversey.BL.Domain.Ideation;

namespace Conversey.REST.Models.Dto;

public class IdeaDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int TopicId { get; set; }
    public string YouthToken { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public ModerationStatus Status { get; set; }
    public IReadOnlyCollection<ResponseReactionSummaryDto> Reactions { get; set; } = Array.Empty<ResponseReactionSummaryDto>();

    public static IdeaDto From(Idea idea)
    {
        return new IdeaDto
        {
            Id = idea.Id,
            Content = idea.Content,
            ProjectId = 0,
            TopicId = idea.Topic.Id,
            YouthToken = idea.Youth.Token.ToString(),
            SubmissionDate = idea.SubmissionDate,
            Status = idea.Status,
            Reactions = ResponseReactionSummaryDto.From(idea.Reactions ?? Array.Empty<IdeaReaction>())
        };
    }
}
