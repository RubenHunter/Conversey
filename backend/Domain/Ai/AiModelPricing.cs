namespace Conversey.BL.Domain.Ai;

public class AiModelPricing
{
    public int Id { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal InputPricePerMillionTokens { get; set; }
    public decimal OutputPricePerMillionTokens { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
