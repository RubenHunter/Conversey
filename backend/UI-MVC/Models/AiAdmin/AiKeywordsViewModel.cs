namespace Conversey.UI_MVC.Models.AiAdmin;

public class AiKeywordsViewModel
{
    public IReadOnlyList<AiKeywordItem> Keywords { get; set; } = Array.Empty<AiKeywordItem>();
}

public class AiKeywordItem
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string? WorkspaceId { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
