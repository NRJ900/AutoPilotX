using System.Collections.Generic;
using System.Windows.Input;
using AutoPilotX.Models;
using AutoPilotX.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls;

namespace AutoPilotX.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private SettingsModel _settings;

        [ObservableProperty]
        private List<string> _themes = new List<string> { "Light", "Dark" };

        [ObservableProperty]
        private bool _isDarkMode;

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = _settingsService.LoadSettings();
            IsDarkMode = Settings.Theme == "Dark";
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        public ICommand SaveSettingsCommand { get; }

        public void SaveSettings()
        {
            Settings.Theme = IsDarkMode ? "Dark" : "Light";
            _settingsService.SaveSettings(Settings);
            _settingsService.SetStartup(Settings.StartWithWindows);
            ThemeManager.Current.ChangeTheme(Application.Current, $"{Settings.Theme}.Blue");
        }
    }
}
