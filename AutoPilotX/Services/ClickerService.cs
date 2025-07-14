using System;
using System.Threading;
using System.Threading.Tasks;
using InputSimulator;
using InputSimulator.Native;

namespace AutoPilotX.Services
{
    public class ClickerService : IDisposable
    {
        private readonly IInputSimulator _inputSimulator = new InputSimulator();
        private CancellationTokenSource _cancellationTokenSource;

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
