using Conversey.BL.Domain.Ideation;

namespace Conversey.REST.Models.Dto;

public class ResponseDto
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid YouthId { get; set; }
    public ModerationStatus Status { get; set; }
    public IEnumerable<ReactionDto> Reactions { get; set; }

    public static ResponseDto From(Response response)
    {
        return new ResponseDto
        {
            Id = response.Id,
            IdeaId = response.Idea.Id,
            Text = response.Text,
            CreatedAt = response.CreatedAt,
            YouthId = response.Youth.Id,
            Status = response.Status,
            Reactions = ReactionDto.From(response.Reactions ?? Array.Empty<ResponseReaction>())
        };
    }
}
