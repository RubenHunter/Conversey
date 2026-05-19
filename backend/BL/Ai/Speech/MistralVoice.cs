namespace Conversey.BL.Ai.Speech;

public class MistralVoice
{
    public string Name { get; }

    private MistralVoice(string name)
    {
        Name = name;
    }

    public static readonly MistralVoice EnPaulNeutral = new("en_paul_neutral");
    public static readonly MistralVoice FrMarieNeutral = new("fr_marie_neutral");
    
    public override string ToString() => Name;
}