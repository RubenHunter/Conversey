namespace Conversey.BL.Domain.Ai.Speech;

public class AudioMimeType
{
    public string Value { get; }
    public string FileExtension { get; }

    private AudioMimeType(string value, string fileExtension)
    {
        Value = value;
        FileExtension = fileExtension;
    }

    public static readonly AudioMimeType Webm = new("audio/webm", "webm");
    public static readonly AudioMimeType Mp3 = new("audio/mp3", "mp3");
    public static readonly AudioMimeType Wav = new("audio/wav", "wav");
    public static readonly AudioMimeType Ogg = new("audio/ogg", "ogg");
    public static readonly AudioMimeType Mp4 = new("audio/mp4", "mp4");
    public static readonly AudioMimeType Mpeg = new("audio/mpeg", "mp3");

    private static readonly IReadOnlyList<AudioMimeType> All = [Webm, Mp3, Wav, Ogg, Mp4, Mpeg];

    public static AudioMimeType FromString(string mimeType)
    {
        var match = All.FirstOrDefault(m => string.Equals(m.Value, mimeType, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            throw new ArgumentException($"Invalid or unsupported MIME type: {mimeType}", nameof(mimeType));
        }
        return match;
    }

    public override string ToString() => Value;
}
