using System.Windows;
using AutoPilotX.Services;
using AutoPilotX.ViewModels;
using AutoPilotX.Views;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoPilotX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddSingleton<SettingsService>();
                    services.AddSingleton<ClickerService>();
                    services.AddSingleton<KeyPresserService>();
                    services.AddSingleton<MacroService>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<AutoClickerViewModel>();
                    services.AddTransient<MacroViewModel>();
                    services.AddTransient<SettingsViewModel>();
                })
                .Build();

            var settingsService = _host.Services.GetRequiredService<SettingsService>();
            var settings = settingsService.LoadSettings();
            ThemeManager.Current.ChangeTheme(this, $"{settings.Theme}.Blue");

            var mainWindow = new MainWindow(
                _host.Services.GetRequiredService<MainViewModel>(),
                _host.Services.GetRequiredService<AutoClickerViewModel>(),
                _host.Services.GetRequiredVewModel<MacroViewModel>(),
                _host.Services.GetRequiredService<SettingsViewModel>()
            );
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
