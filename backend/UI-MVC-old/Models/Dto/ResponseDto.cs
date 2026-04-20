using Conversey.BL.Domain.Ideation;

namespace Conversey.UI_MVC.Models.Dto;

public class ResponseDto
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid YouthId { get; set; }
    public ModerationStatus Status { get; set; }
    public IEnumerable<ReactionDto> Reactions { get; set; }

    public static ResponseDto From(IdeaResponse ideaResponse)
    {
        return new ResponseDto
        {
            Id = ideaResponse.Id,
            IdeaId = ideaResponse.Idea.Id,
            Text = ideaResponse.Text,
            CreatedAt = ideaResponse.CreatedAt,
            YouthId = ideaResponse.Youth.Id,
            Status = ideaResponse.Status,
            Reactions = (ideaResponse.Reactions ?? Array.Empty<ResponseReaction>())
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto { Emoji = g.Key, Count = g.Count() })
        };
    }
}
