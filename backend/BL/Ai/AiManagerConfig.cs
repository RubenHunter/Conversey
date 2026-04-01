using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Conversey.BL.Ai;

public class AiManagerConfig
{
    [Required]
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public string ModerationModel { get; set; }
}