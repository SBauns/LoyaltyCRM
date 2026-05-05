using Microsoft.Extensions.Logging;
using System.Text.Json;

public static class TranslationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
    private static readonly object _lock = new();

    public static string TranslateAndTrack(string key, string lang, ILogger logger)
    {
        var dict = GetTranslationsForLanguage(lang);

        if (dict != null && dict.TryGetValue(key, out var value))
            return value;

        // fallback to English
        var enDict = GetTranslationsForLanguage("en");
        if (enDict != null && enDict.TryGetValue(key, out var fallback))
            return fallback;

        return key;
    }

    private static Dictionary<string, string>? GetTranslationsForLanguage(string lang)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(lang, out var dict))
                return dict;

            var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", $"Translations.{lang}.json");

            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (loaded != null)
                _cache[lang] = loaded;

            return loaded;
        }
    }
}