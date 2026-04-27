using Conversey.BL.Domain.Ideation;

namespace Conversey.UI_MVC.Models.Dto;

public class ReactionDto
{
    public string Emoji { get; set; }
    public int Count { get; set; }

    public static ReactionDto From(ResponseReaction reaction)
    {
        return new ReactionDto{Emoji = reaction.Emoji, Count = 1};
    }

    public static ReactionDto From(IdeaReaction reaction)
    {
        return new ReactionDto{Emoji = reaction.Emoji, Count = 1};
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
