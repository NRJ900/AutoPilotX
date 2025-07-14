using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using AutoPilotX.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsInput.Native;

namespace AutoPilotX.ViewModels
{
    public partial class AutoClickerViewModel : ObservableObject, IDisposable
    {
        private readonly ClickerService _clickerService;
        private bool _disposed;

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
        private VirtualKeyCode _selectedClickType = VirtualKeyCode.LBUTTON;

        [ObservableProperty]
        private int _interval = 100;

        [ObservableProperty]
        private Brush _statusColor = Brushes.Red;

        public AutoClickerViewModel(ClickerService clickerService)
        {
            _clickerService = clickerService;
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

        public void Dispose()
        {
            if (_disposed) return;
            _clickerService.Dispose();
            _disposed = true;
        }

        ~AutoClickerViewModel() => Dispose();
    }
}
