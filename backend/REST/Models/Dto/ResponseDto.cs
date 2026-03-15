using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.REST.Models.Dto;

public class ResponseDto
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string YouthToken { get; set; } = string.Empty;
    public IReadOnlyCollection<ResponseReactionSummaryDto> Reactions { get; set; } = Array.Empty<ResponseReactionSummaryDto>();

    public static ResponseDto From(Response response)
    {
        return new ResponseDto
        {
            Id = response.Id,
            IdeaId = response.Idea.Id,
            Text = response.Text,
            CreatedAt = response.CreatedAt,
            YouthToken = response.Youth.Token,
            Reactions = ResponseReactionSummaryDto.From(response.Reactions ?? Array.Empty<ResponseReaction>())
        };
    }
}

