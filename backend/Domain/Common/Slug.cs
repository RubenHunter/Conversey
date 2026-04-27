using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Conversey.BL.Domain.Common;

[TypeConverter(typeof(SlugTypeConverter))]
public record struct Slug
{
    public string Text;

    public override string ToString() => Text ?? string.Empty;

    public static Slug FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new Slug { Text = "" };
        
        string cleaned = name.Trim().ToLower().Replace(" ", "-");
        cleaned = Regex.Replace(cleaned, @"[^a-z0-9_-]", "");
        
        return new Slug { Text = cleaned };
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