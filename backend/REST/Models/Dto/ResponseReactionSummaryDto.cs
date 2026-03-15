using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.REST.Models.Dto;

public class ResponseReactionSummaryDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }

    public static IReadOnlyCollection<ResponseReactionSummaryDto> From(IEnumerable<ResponseReaction> reactions)
    {
        return FromEmojis(reactions.Select(reaction => reaction.Emoji));
    }

    public static IReadOnlyCollection<ResponseReactionSummaryDto> From(IEnumerable<IdeaReaction> reactions)
    {
        return FromEmojis(reactions.Select(reaction => reaction.Emoji));
    }

    public static IReadOnlyCollection<ResponseReactionSummaryDto> FromEmojis(IEnumerable<string> emojis)
    {
        return emojis
            .Where(emoji => !string.IsNullOrWhiteSpace(emoji))
            .GroupBy(emoji => emoji)
            .Select(group => new ResponseReactionSummaryDto
            {
                Emoji = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(summary => summary.Count)
            .ThenBy(summary => summary.Emoji)
            .ToList()
            .AsReadOnly();
    }
}
