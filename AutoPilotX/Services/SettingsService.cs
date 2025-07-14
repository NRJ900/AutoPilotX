using System.IO;
using System.Text.Json;
using AutoPilotX.Models;
using Microsoft.Win32;

namespace AutoPilotX.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private const string AppName = "AutoPilotX";

        public SettingsService()
        {
            var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), AppName);
            Directory.CreateDirectory(appDataPath);
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }

        public void SaveSettings(SettingsModel settings)
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(tempPath, json);
                File.Replace(tempPath, _settingsFilePath, null);
            }
            catch (JsonException)
            {
                // Ignore serialization errors
            }
            catch (IOException)
            {
                // Ignore file operation errors
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (IOException)
                    {
                        // Ignore file deletion errors
                    }
                }
            }
        }

        public SettingsModel LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new SettingsModel();
                }

                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<SettingsModel>(json);
            }
            catch (JsonException)
            {
                // Ignore deserialization errors
                return new SettingsModel();
            }
            catch (IOException)
            {
                // Ignore file operation errors
                return new SettingsModel();
            }
        }

        public void SetStartup(bool startWithWindows)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (startWithWindows)
                {
                    key.SetValue(AppName, System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (System.Exception)
            {
                // Ignore registry errors
            }
        }
    }
}
