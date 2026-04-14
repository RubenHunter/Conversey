using Conversey.BL.Domain.Ideation;

namespace Conversey.REST.Models.Dto;

public class ReactionDto
{
    public string Emoji { get; set; }

    public static ReactionDto From(ResponseReaction reaction)
    {
        return new ReactionDto{Emoji = reaction.Emoji};
    }

    public static ReactionDto From(IdeaReaction reaction)
    {
        return new ReactionDto{Emoji = reaction.Emoji};
    }

    /*public static IEnumerable<ReactionDto> FromEmoji(string emoji)
    {
        return emojis
            .Where(emoji => !string.IsNullOrWhiteSpace(emoji))
            .GroupBy(emoji => emoji)
            .Select(group => new ReactionDto
            {
                Emoji = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(summary => summary.Count)
            .ThenBy(summary => summary.Emoji)
            .ToList()
            .AsReadOnly();
    }*/
}
