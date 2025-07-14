using System.Windows;
using AutoPilotX.ViewModels;
using MahApps.Metro.Controls;

namespace AutoPilotX.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainViewModel mainViewModel, AutoClickerViewModel autoClickerViewModel, MacroViewModel macroViewModel, SettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
            AutoClickerView.DataContext = autoClickerViewModel;
            MacroView.DataContext = macroViewModel;
            SettingsView.DataContext = settingsViewModel;
        }

        private void MetroWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (FindResource("NotifyIcon") is System.Windows.Forms.NotifyIcon notifyIcon)
                {
                    notifyIcon.Visible = true;
                }
            }
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            if (FindResource("NotifyIcon") is System.Windows.Forms.NotifyIcon notifyIcon)
            {
                notifyIcon.Visible = false;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            if (FindResource("NotifyIcon") is System.Windows.Forms.NotifyIcon notifyIcon)
            {
                notifyIcon.Visible = false;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
