namespace Conversey.BL.Domain.Common;

public record struct Slug
{
    public string Text;

    public static Slug FromName(string name)
    {
        return new Slug
        {
            Text = name
        };
    }
}