using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

using System.Text.Json.Serialization;

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
}