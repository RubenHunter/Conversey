namespace Conversey.BL.Ai;

public static class PromptRenderer
{
    public static string Render(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        var result = template;
        foreach (var (key, value) in variables)
        {
            var sanitized = (value ?? string.Empty)
                .Replace("{{", "{ {")
                .Replace("}}", "} }");
            result = result.Replace($"{{{{{key}}}}}", sanitized);
        }

        return result;
    }
}
