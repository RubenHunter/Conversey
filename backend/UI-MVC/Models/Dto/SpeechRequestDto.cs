using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Models.Dto;

public class SpeechTranscribeRequest
{
    [Required]
    public string AudioBase64 { get; set; } = string.Empty;
    public Language Language { get; set; } = Language.nl;
    public IEnumerable<string> ContextBias { get; set; } = Array.Empty<string>();
    public string MimeType { get; set; } = "audio/webm";
}

public class TextSynthesizeRequest
{
    [Required]
    public string Input { get; set; } = string.Empty;
    public Language Language { get; set; } = Language.nl;
}
