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
    public ModerationCategories Categories { get; set; } = new();

    [JsonPropertyName("category_scores")]
    public ModerationCategoryScores CategoryScores { get; set; } = new();
}

public class ModerationCategories
{
    [JsonPropertyName("sexual")]
    public bool Sexual { get; set; }

    [JsonPropertyName("hate_and_discrimination")]
    public bool HateAndDiscrimination { get; set; }

    [JsonPropertyName("violence_and_threats")]
    public bool ViolenceAndThreats { get; set; }

    [JsonPropertyName("dangerous_and_criminal_content")]
    public bool DangerousAndCriminalContent { get; set; }

    [JsonPropertyName("selfharm")]
    public bool SelfHarm { get; set; }

    [JsonPropertyName("pii")]
    public bool Pii { get; set; }
}

public class ModerationCategoryScores
{
    [JsonPropertyName("sexual")]
    public double Sexual { get; set; }

    [JsonPropertyName("hate_and_discrimination")]
    public double HateAndDiscrimination { get; set; }

    [JsonPropertyName("violence_and_threats")]
    public double ViolenceAndThreats { get; set; }

    [JsonPropertyName("dangerous_and_criminal_content")]
    public double DangerousAndCriminalContent { get; set; }

    [JsonPropertyName("selfharm")]
    public double SelfHarm { get; set; }

    [JsonPropertyName("pii")]
    public double Pii { get; set; }
}