using System.Text.RegularExpressions;

namespace Conversey.BL.Domain.Common;

public record struct Slug
{
    public string Text;

    public static Slug FromName(string name)
    {
        return new Slug
        {
            Text = Regex.Replace(name.Trim().ToLower().Replace(" ", "-"), @"[^a-z0-9_-]", "")
        };
    }
}