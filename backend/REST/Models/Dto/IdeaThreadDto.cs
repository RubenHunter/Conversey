using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.REST.Models.Dto;

public class IdeaThreadDto
{
    public IdeaDto Idea { get; set; } = new();
    public IReadOnlyCollection<ResponseDto> Responses { get; set; } = Array.Empty<ResponseDto>();

    public static IdeaThreadDto From(Idea idea)
    {
        return new IdeaThreadDto
        {
            Idea = IdeaDto.From(idea),
            Responses = (idea.Responses ?? Array.Empty<Response>())
                .OrderBy(response => response.CreatedAt)
                .ThenBy(response => response.Id)
                .Select(ResponseDto.From)
                .ToList()
                .AsReadOnly()
        };
    }
}

