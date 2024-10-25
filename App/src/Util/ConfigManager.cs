using System.Reflection;
using Newtonsoft.Json.Linq;

namespace App.Util;

public static class ConfigManager
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    private class ConfigPropertyAttribute : Attribute;

    private static JObject _config = null!;

    [ConfigProperty] public static int Language = 0;
    [ConfigProperty] public static int MaxConsoleLines = 500;
    [ConfigProperty] public static float WindowTransparency = 0.8f;

    private static IEnumerable<MemberInfo> GetConfigProperties()
    {
        var type = typeof(ConfigManager);
        return type.GetMembers(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.GetCustomAttribute<ConfigPropertyAttribute>() != null);
    }

    public static void Save()
    {
        var properties = GetConfigProperties();
        foreach (var property in properties)
        {
            var key = property.Name;
            var value = property switch
            {
                PropertyInfo pi => pi.GetValue(null),
                FieldInfo fi => fi.GetValue(null),
                _ => throw new InvalidOperationException()
            };
            if (value == null)
                continue;
            
            _config[key] = JToken.FromObject(value);
        }


        var json = _config.ToString();
        File.WriteAllText("config.json", json);
    }

    public static void Load()
    {
        if (!File.Exists("config.json"))
        {
            _config = new JObject();
            Save();
            return;
        }

        var json = File.ReadAllText("config.json");
        _config = JObject.Parse(json);

        var properties = GetConfigProperties();
        foreach (var property in properties)
        {
            var key = property.Name;
            if (_config.TryGetValue(key, out var value))
            {
                switch (property)
                {
                    case PropertyInfo pi:
                        pi.SetValue(null, value.ToObject(pi.PropertyType));
                        break;
                    case FieldInfo fi:
                        fi.SetValue(null, value.ToObject(fi.FieldType));
                        break;
                }
            }
        }
    }

    static ConfigManager()
    {
        Load();
    }
}