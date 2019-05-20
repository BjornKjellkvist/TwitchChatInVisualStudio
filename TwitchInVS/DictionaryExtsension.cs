using System.Collections.Generic;

namespace TwitchInVS
{
    public static class DictionaryExtsension
    {
        public static void Update(this Dictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static string GetSafe(this Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
