namespace Conversey.UI_MVC.Models.Dto;

public class TranscribeRequest
{
    public string AudioBase64 { get; set; } = string.Empty;
    public string Language { get; set; }
    public IEnumerable<string> ContextBias { get; set; } = Array.Empty<string>();
    public string MimeType { get; set; } = "audio/webm";
    public bool Stream { get; set; } = false;
}

public class SynthesizeRequest
{
    public string Model { get; set; }
    public string Input { get; set; } = string.Empty;
    public string Language { get; set; }
}
