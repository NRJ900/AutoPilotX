using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.Services
{
    public class ClickerService
    {
        private readonly InputSimulator _inputSimulator;
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
                    _inputSimulator.Mouse.Click(mouseButton);
                    await Task.Delay(interval, token);
                }
            }, token);
        }

        public void StopClicking()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
