using System;
using System.IO;
using System.Text.Json;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class AppSettings
    {
        public bool AlwaysOnTop { get; set; } = true;
        public bool MinimizeToTray { get; set; } = false;
        public string Theme { get; set; } = "Dark";
        public bool SoundEffects { get; set; } = true;
    }

    public class SettingsService
    {
        private const string SettingsFile = "settings.json";
        public AppSettings Settings { get; private set; } = new AppSettings();

        public event EventHandler? SettingsChanged;

        public SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFile);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        Settings = loaded;
                        SettingsChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load settings", ex);
                }
            }
        }

        public void SaveSettings(AppSettings newSettings)
        {
            try
            {
                Settings = newSettings;
                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
                
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save settings", ex);
            }
        }
        
        // Overload to save current
        public void SaveSettings()
        {
             SaveSettings(Settings);
        }
    }
}
