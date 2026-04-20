using System.ComponentModel;
using System.Globalization;
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

public class SlugTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string s)
        {
            return Slug.FromName(s);
        }

        return base.ConvertFrom(context, culture, value);
    }
}