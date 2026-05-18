using System.Text.Json.Serialization;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

public class ModerationResponse
{
    [JsonPropertyName("results")]
    public ModerationResult[] Results { get; set; } = Array.Empty<ModerationResult>();
}

public class ModerationResult
{
    [JsonPropertyName("categories")]
    public ModerationInfo Categories { get; set; } = new();
}

public class ModerationDecision
{
    public bool IsAllowed { get; set; }
    public ModerationInfo Categories { get; set; } = new();
    public string Suggestion { get; set; }
}

public class IdeaNudgeTurn
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

public class IdeaNudgeAssessmentRequest
{
    public string ProjectTitle { get; set; }
    public string ProjectDescription { get; set; }
    public string TopicTitle { get; set; }
    public string TopicPrompt { get; set; }
    public string IdeaText { get; set; }
    public IReadOnlyList<IdeaNudgeTurn> Conversation { get; set; } = Array.Empty<IdeaNudgeTurn>();
    public string NudgingMode { get; set; }
}

public class IdeaNudgeDecision
{
    public bool IsApproved { get; set; }
    public string Question { get; set; }
}

