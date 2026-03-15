using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.REST.Models.Dto;

public class IdeaDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int TopicId { get; set; }
    public string YouthToken { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public IdeaStatus Status { get; set; }

    public static IdeaDto From(Idea idea)
    {
        return new IdeaDto
        {
            Id = idea.Id,
            Content = idea.Content,
            ProjectId = idea.ProjectId,
            TopicId = idea.TopicId,
            YouthToken = idea.YouthToken,
            SubmissionDate = idea.SubmissionDate,
            Status = idea.Status
        };
    }
}