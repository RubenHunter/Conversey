using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Ai;

public class AiManagerConfig
{
    [Required]
    public string ApiKey { get; set; }
    public string CompletionsModel { get; set; }
    public string ModerationModel { get; set; }
}