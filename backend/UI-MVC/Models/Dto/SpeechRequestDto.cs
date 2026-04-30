namespace Conversey.UI_MVC.Models.Dto;

public class SpeechTranscribeRequest
{
    public string AudioBase64 { get; set; } = string.Empty;
    public string Language { get; set; }
    public IEnumerable<string> ContextBias { get; set; } = Array.Empty<string>();
    public string MimeType { get; set; } = "audio/webm";
}

public class TextSynthesizeRequest
{
    public string Input { get; set; } = string.Empty;
    public string Language { get; set; }
}
