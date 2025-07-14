using System.Windows;
using MahApps.Metro.Controls;

namespace AutoPilotX.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                NotifyIcon.Visibility = Visibility.Visible;
            }
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            NotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            NotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
