using System.Text.Json.Serialization;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public struct ModerationInfo
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

    public byte Serialize()
    {
        return (byte)(ToByte(Sexual) |
                      ToByte(HateAndDiscrimination) << 1 |
                      ToByte(ViolenceAndThreats) << 2 |
                      ToByte(DangerousAndCriminalContent) << 3 |
                      ToByte(SelfHarm) << 4 |
                      ToByte(Pii) << 5);
    }

    public static ModerationInfo Deserialize(byte value)
    {
        return new ModerationInfo
        {
            Sexual = ToBool(value),
            HateAndDiscrimination = ToBool(value >> 1),
            ViolenceAndThreats =  ToBool(value >> 2),
            DangerousAndCriminalContent =  ToBool(value >> 3),
            SelfHarm =  ToBool(value >> 4),
            Pii =  ToBool(value >> 5),
        };
    }
    
    private static bool ToBool(int v)
    {
        return v != 0;
    }

    private static byte ToByte(bool v)
    {
        return (byte)(v ? 1 : 0);
    }
}