using System.IO;
using System.Text.Json;
using AutoPilotX.Models;

namespace AutoPilotX.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "AutoPilotX");
            Directory.CreateDirectory(appDataPath);
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }

        public void SaveSettings(SettingsModel settings)
        {
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
        }

        public SettingsModel LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new SettingsModel();
            }

            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<SettingsModel>(json);
        }
    }
}
