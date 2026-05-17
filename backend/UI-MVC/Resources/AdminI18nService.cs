using System.Collections.Concurrent;
using System.Text.Json;

namespace Conversey.UI_MVC.Resources;

public interface IAdminI18nService
{
    string this[string key] { get; }
    string CurrentLanguage { get; }
    IReadOnlyList<string> SupportedLanguages { get; }
    IReadOnlyDictionary<string, string> CurrentLanguageMap { get; }
}

public class AdminI18nService : IAdminI18nService
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _allStrings = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly string[] Supported = ["en", "nl", "fr"];

    public IReadOnlyList<string> SupportedLanguages => Supported;

    public string CurrentLanguage
    {
        get
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            var lang = culture.TwoLetterISOLanguageName;
            return Supported.Contains(lang) ? lang : "en";
        }
    }

    public string this[string key]
    {
        get
        {
            var lang = CurrentLanguage;
            if (_allStrings.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
            {
                return value;
            }
            if (lang != "en" && _allStrings.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var enValue))
            {
                return enValue;
            }
            return key;
        }
    }

    public IReadOnlyDictionary<string, string> CurrentLanguageMap
    {
        get
        {
            var lang = CurrentLanguage;
            if (_allStrings.TryGetValue(lang, out var dict))
            {
                return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(dict);
            }

            if (_allStrings.TryGetValue("en", out var enDict))
            {
                return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(enDict);
            }

            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }
    }

    public AdminI18nService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        foreach (var lang in Supported)
        {
            var dict = LoadJson(env, lang);
            if (dict != null)
            {
                _allStrings[lang] = dict;
            }
        }
    }

    private static Dictionary<string, string>? LoadJson(IWebHostEnvironment env, string language)
    {
        var path = Path.Combine(env.ContentRootPath, "Resources", $"admin-{language}.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }
}
