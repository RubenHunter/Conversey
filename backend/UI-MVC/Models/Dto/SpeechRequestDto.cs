namespace Conversey.UI_MVC.Models.Dto;

public class TranscribeRequest
{
    public string AudioBase64 { get; set; } = string.Empty;
    public string Language { get; set; }
    public string Prompt { get; set; }
}

public class SynthesizeRequest
{
    public string Text { get; set; } = string.Empty;
    public string Language { get; set; }
}
