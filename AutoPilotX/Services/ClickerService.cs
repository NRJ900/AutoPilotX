using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.Services
{
    public class ClickerService
    {
        private readonly IInputSimulator _inputSimulator;
        private CancellationTokenSource _cancellationTokenSource;

        public ClickerService(IInputSimulator inputSimulator)
        {
            _inputSimulator = inputSimulator;
        }

        public void Start(int interval, VirtualKeyCode clickType, int x, int y)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ClickerLoop(interval, clickType, x, y, _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task ClickerLoop(int interval, VirtualKeyCode clickType, int x, int y, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (x != -1 && y != -1)
                {
                    _inputSimulator.Mouse.MoveMouseTo(x, y);
                }
                _inputSimulator.Mouse.Click(clickType);
                await Task.Delay(interval, cancellationToken);
            }
        }
    }
}
