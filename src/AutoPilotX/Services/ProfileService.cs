using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class ProfileService
    {
        private const string ProfilesDir = "profiles";
        private const string CurrentProfileFile = "current_profile.txt";
        private const string DefaultProfileName = "Default";

        public string CurrentProfile { get; private set; } = DefaultProfileName;
        
        private readonly MacroService _macro;
        private readonly HotkeyService _hotkey;
        private readonly StatsService _stats;
        private readonly SettingsService _settings;

        // Files to manage
        private readonly string[] _managedFiles = { "macros.json", "hotkeys.json", "stats.json", "settings.json" };

        public event EventHandler? ProfileChanged;

        public ProfileService(MacroService macro, HotkeyService hotkey, StatsService stats, SettingsService settings)
        {
            _macro = macro;
            _hotkey = hotkey;
            _stats = stats;
            _settings = settings;

            if (!Directory.Exists(ProfilesDir)) Directory.CreateDirectory(ProfilesDir);
            
            // Load last active profile name
            if (File.Exists(CurrentProfileFile))
            {
                string stored = File.ReadAllText(CurrentProfileFile).Trim();
                if (!string.IsNullOrWhiteSpace(stored) && (Directory.Exists(Path.Combine(ProfilesDir, stored)) || stored == DefaultProfileName))
                {
                    CurrentProfile = stored;
                }
            }
        }

        public List<string> GetProfiles()
        {
            var dirs = Directory.GetDirectories(ProfilesDir)
                .Select(Path.GetFileName)
                .ToList();
            
            if (!dirs.Contains(DefaultProfileName)) dirs.Insert(0, DefaultProfileName);
            return dirs;
        }

        public void CreateProfile(string name)
        {
            string path = Path.Combine(ProfilesDir, name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                // Optionally copy current files to new profile?
                // Or start empty. Let's start empty.
            }
        }

        public void DeleteProfile(string name)
        {
            if (name == DefaultProfileName) return; // Cannot delete default
            if (name == CurrentProfile) SwitchProfile(DefaultProfileName);

            string path = Path.Combine(ProfilesDir, name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public void SwitchProfile(string targetName)
        {
            if (CurrentProfile == targetName) return;

            Logger.Info($"Switching profile from {CurrentProfile} to {targetName}");

            // 1. Save all services to disk (root)
            _macro.SaveMacros();
            _hotkey.SaveHotkeys();
            _stats.SaveStats();
            _settings.SaveSettings();

            // 2. Backup root files to CurrentProfile folder
            string currentPath = Path.Combine(ProfilesDir, CurrentProfile);
            if (!Directory.Exists(currentPath)) Directory.CreateDirectory(currentPath);

            foreach (var file in _managedFiles)
            {
                if (File.Exists(file))
                {
                    File.Copy(file, Path.Combine(currentPath, file), true);
                }
            }

            // 3. Prepare Target Profile
            string targetPath = Path.Combine(ProfilesDir, targetName);
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            // 4. Overwrite root files with Target files
            foreach (var file in _managedFiles)
            {
                string source = Path.Combine(targetPath, file);
                if (File.Exists(source))
                {
                    File.Copy(source, file, true);
                }
                else
                {
                    // Target doesn't have this file, delete root so we start fresh?
                    // Or keep existing?
                    // Better to delete root to avoid leaking state from previous profile
                    if (File.Exists(file)) File.Delete(file);
                }
            }

            // 5. Update Current Profile Tracker
            CurrentProfile = targetName;
            File.WriteAllText(CurrentProfileFile, CurrentProfile);

            // 6. Reload Services
            _macro.LoadMacros();
            _hotkey.LoadHotkeys();
            _stats.LoadStats();
            _settings.LoadSettings();

            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
