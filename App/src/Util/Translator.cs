using System.Text.Json;

namespace App.Util;

public static class Translator
{
    private static readonly string[] Languages = ["en_us", "zh_cn"];
    public static readonly string[] LanguageText = ["English | en_us", "简体中文 | zh_cn"];
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new();
    
    public static int CurrentLanguageIndex => ConfigManager.Language;

    static Translator()
    {
        foreach (var language in Languages)
        {
            foreach (var lang in Languages)
            {
                var s = AppRes.ResourceManager.GetString($"lang_{lang}");
                if (s == null)
                    continue;

                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(s);
                if (json == null)
                    continue;

                var translations = new Dictionary<string, string>();
                Translations[lang] = translations;
                foreach (var (key, value) in json)
                {
                    translations[key] = value;
                }
            }
        }
    }

    public static string GetTranslation(string key, params object[] fmtargs)
    {
        if (Translations[Languages[CurrentLanguageIndex]].TryGetValue(key, out var value))
        {
            return string.Format(value, fmtargs);
        }

        return Translations[Languages[0]].TryGetValue(key, out var value2) ? string.Format(value2, fmtargs) : key;
    }
}