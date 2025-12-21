using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoPilotX.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void Save<T>(string path, T data)
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(data, Options);
            File.WriteAllText(path, json);
        }

        public static T? Load<T>(string path)
        {
            if (!File.Exists(path)) return default;

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
