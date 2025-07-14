using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.Services
{
    public class ClickerService : IDisposable
    {
        private readonly IInputSimulator _inputSimulator;
        private CancellationTokenSource _cancellationTokenSource;

        public ClickerService()
        {
            _inputSimulator = new InputSimulator();
        }

        public async Task StartClicking(int interval, VirtualKeyCode mouseButton)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        _inputSimulator.Mouse.MouseClick(mouseButton);
                    }
                    catch (Exception)
                    {
                        // Ignore input simulation errors
                    }
                    await Task.Delay(interval, token);
                }
            }, token);
        }

        public void StopClicking()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}
