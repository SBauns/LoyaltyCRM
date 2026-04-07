using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace PapasCRM_API.Services
{
    public static class TranslationService
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
        private static readonly object _lock = new();

        public static IHttpContextAccessor? HttpContextAccessor { get; set; }

        public static string Translate(string key)
        {
            var lang = GetLanguage();
            var dict = GetTranslationsForLanguage(lang);
            if (dict != null && dict.TryGetValue(key, out var value))
                return value;
            // fallback to English
            var enDict = GetTranslationsForLanguage("en");
            if (enDict != null && enDict.TryGetValue(key, out var fallback))
                return fallback;
            return key;
        }

        private static string GetLanguage()
        {
            var httpContext = HttpContextAccessor?.HttpContext;
            var acceptLang = httpContext?.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLang))
            {
                return acceptLang.Split(',')[0].Split('-')[0];
            }
            return "en"; // default
        }

        private static Dictionary<string, string>? GetTranslationsForLanguage(string lang)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(lang, out var dict))
                    return dict;
                var filePath = $"Resources/Translations.{lang}.json";
                if (!System.IO.File.Exists(filePath))
                    return null;
                var json = System.IO.File.ReadAllText(filePath);
                var loaded = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (loaded != null)
                    _cache[lang] = loaded;
                return loaded;
            }
        }
    }
}
