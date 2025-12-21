using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoPilotX.Services;
using AutoPilotX.Models;

namespace AutoPilotX
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        private readonly AutoClickerService _autoClicker;
        private readonly HotkeyService _hotkeyService;

        private readonly MacroService _macroService;
        private readonly StatsService _statsService;
        private readonly ProfileService _profileService;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;
        private readonly Action<string, object> _emitEvent;

        public Bridge(
            AutoClickerService autoClicker, 
            HotkeyService hotkeyService, 
            MacroService macroService,
            StatsService statsService,
            ProfileService profileService,
            SettingsService settingsService,
            Action<string, object> emitEvent)
        {
            _autoClicker = autoClicker;
            _hotkeyService = hotkeyService;
            _macroService = macroService;
            _statsService = statsService;
            _profileService = profileService;
            _settingsService = settingsService;
            _updateService = new UpdateService();
            _emitEvent = emitEvent;

            // Wire up events
            _autoClicker.Started += (s, e) => Emit("AutoClickerState", new { isRunning = true });
            _autoClicker.Stopped += (s, e) => Emit("AutoClickerState", new { isRunning = false });
            
            _macroService.RecordingStatusChanged += (s, status) => Emit("MacroRecordingStatus", new { status });
            _macroService.MacrosChanged += (s, e) => Emit("MacrosListChanged", null); // Auto-refresh list
            _hotkeyService.HotkeysChanged += (s, e) => Emit("HotkeysListChanged", null); // Auto-refresh hotkeys
        }

        private void Emit(string eventName, object? data)
        {
            _emitEvent?.Invoke(eventName, data ?? new object());
        }

        // ... AutoClicker methods ...

        public void PrepareRecording(string name, bool keys, bool mouse)
        {
            _macroService.PrepareRecording(name, keys, mouse);
        }

        public void ToggleRecording()
        {
            _macroService.ToggleRecording();
        }

        public void CancelRecording()
        {
            _macroService.CancelRecording();
        }

        public void StartAutoClicker(string jsonSettings)
        {
            try 
            {
                var options = new System.Text.Json.JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

                var settings = System.Text.Json.JsonSerializer.Deserialize<AutoClickerSettings>(jsonSettings, options);
                if (settings != null)
                {
                    _autoClicker.Start(settings);
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Failed to parse AC settings", ex);
            }
        }

        public void StopAutoClicker()
        {
            _autoClicker.Stop();
        }
        
        public bool IsAutoClickerRunning() => _autoClicker.IsRunning;
        
        public async Task<string> PickLocation()
        {
             var (x, y) = await _autoClicker.PickLocation();
             return $"{x},{y}";
        }

        // Macros
        public string GetMacros()
        {
            return System.Text.Json.JsonSerializer.Serialize(_macroService.Macros);
        }

        // Legacy shims removed, using Prepare/Cancel now
        // But logic in Service kept for compatibility if needed
        // Just cleaning up Bridge to force UI to use new flow


        public void PlayMacro(string name)
        {
            var macro = _macroService.Macros.Find(m => m.Name == name);
            if (macro != null)
            {
                // Play in background to not block UI
                Task.Run(async () => await _macroService.StartMacro(macro));
            }
        }

        public void DeleteMacro(string name)
        {
            var macro = _macroService.Macros.Find(m => m.Name == name);
            if (macro != null)
            {
                _macroService.RemoveMacro(macro);
            }
        }

        public void UpdateMacro(string json)
        {
            try 
            {
                var macro = System.Text.Json.JsonSerializer.Deserialize<Macro>(json);
                if (macro != null)
                {
                    _macroService.UpdateMacro(macro);
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Failed to update macro", ex);
            }
        }

        // Hotkeys
        public string GetHotkeys()
        {
            return System.Text.Json.JsonSerializer.Serialize(_hotkeyService.Definitions);
        }

        public void SetHotkey(string id, int key, int modifiers)
        {
            _hotkeyService.SetBinding(id, (Keys)key, (Keys)modifiers);
        }

        public void CreateMacro(string name)
        {
            _macroService.AddMacro(new Macro(name));
        }

        // Stats
        public string GetStats()
        {
            return System.Text.Json.JsonSerializer.Serialize(_statsService.Stats);
        }

        // Profiles
        public string GetProfiles()
        {
            return System.Text.Json.JsonSerializer.Serialize(_profileService.GetProfiles());
        }

        public string GetCurrentProfile() => _profileService.CurrentProfile;

        public void CreateProfile(string name) => _profileService.CreateProfile(name);
        public void SwitchProfile(string name) => _profileService.SwitchProfile(name);
        public void DeleteProfile(string name) => _profileService.DeleteProfile(name);

        // Settings
        public string GetSettings()
        {
            return System.Text.Json.JsonSerializer.Serialize(_settingsService.Settings);
        }

        public void SaveSettings(string json)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json, options);
                if (settings != null)
                {
                    _settingsService.SaveSettings(settings);
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Failed to save settings", ex);
            }
        }

        public async Task<string> CheckUpdates()
        {
             var info = await _updateService.CheckForUpdates();
             return System.Text.Json.JsonSerializer.Serialize(info);
        }

        public void OpenUpdateUrl(string url)
        {
            _updateService.OpenDownloadPage(url);
        }
    }
}
