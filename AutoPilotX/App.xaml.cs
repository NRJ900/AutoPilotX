using System.Windows;
using AutoPilotX.Models;
using AutoPilotX.Services;
using MahApps.Metro.Controls;
using ControlzEx.Theming;

namespace AutoPilotX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settingsService = new SettingsService();
            var settings = settingsService.LoadSettings();

            ThemeManager.Current.ChangeTheme(this, $"{settings.Theme}.Blue");
        }
    }
}
