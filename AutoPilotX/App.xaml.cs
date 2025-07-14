using System.Windows;
using AutoPilotX.Services;
using AutoPilotX.ViewModels;
using AutoPilotX.Views;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPilotX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SettingsService>();
            services.AddSingleton<ClickerService>();
            services.AddSingleton<KeyPresserService>();
            services.AddSingleton<MacroService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<AutoClickerViewModel>();
            services.AddSingleton<MacroViewModel>();
            services.AddSingleton<SettingsViewModel>();

            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settingsService = _serviceProvider.GetService<SettingsService>();
            var settings = settingsService.LoadSettings();

            ThemeManager.Current.ChangeTheme(this, $"{settings.Theme}.Blue");

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
