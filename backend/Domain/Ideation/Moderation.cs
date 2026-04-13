namespace Conversey.BL.Domain.Ideation;

public struct ModerationInfo
{
    public bool Sexual { get; set; }

    public bool HateAndDiscrimination { get; set; }

    public bool ViolenceAndThreats { get; set; }

    public bool DangerousAndCriminalContent { get; set; }

    public bool SelfHarm { get; set; }

    public bool Pii { get; set; }

    //TODO split this into a different class
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

public enum ModerationStatus
{
    Pending,
    Approved,
    Rejected,
}