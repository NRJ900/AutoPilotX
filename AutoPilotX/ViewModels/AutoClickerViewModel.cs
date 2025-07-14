using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using AutoPilotX.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsInput.Native;

namespace AutoPilotX.ViewModels
{
    public partial class AutoClickerViewModel : ObservableObject
    {
        private readonly ClickerService _clickerService;

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private List<VirtualKeyCode> _clickTypes = new List<VirtualKeyCode>
        {
            VirtualKeyCode.LBUTTON,
            VirtualKeyCode.RBUTTON,
            VirtualKeyCode.MBUTTON
        };

        [ObservableProperty]
        private VirtualKeyCode _selectedClickType;

        [ObservableProperty]
        private int _interval = 100;

        [ObservableProperty]
        private Brush _statusColor = Brushes.Red;

        public AutoClickerViewModel()
        {
            _clickerService = new ClickerService();
            StartClickerCommand = new RelayCommand(StartClicker, () => IsEnabled);
            StopClickerCommand = new RelayCommand(StopClicker);
        }

        public ICommand StartClickerCommand { get; }
        public ICommand StopClickerCommand { get; }

        private async void StartClicker()
        {
            StatusColor = Brushes.Green;
            await _clickerService.StartClicking(Interval, SelectedClickType);
        }

        private void StopClicker()
        {
            _clickerService.StopClicking();
            StatusColor = Brushes.Red;
        }
    }
}
