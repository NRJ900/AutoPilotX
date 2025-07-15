using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.Services
{
    public class KeyPresserService
    {
        private readonly IInputSimulator _inputSimulator;
        private CancellationTokenSource _cancellationTokenSource;

        public KeyPresserService(IInputSimulator inputSimulator)
        {
            _inputSimulator = inputSimulator;
        }

        public void Start(List<VirtualKeyCode> keys, int delay, int loopCount)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => KeyPresserLoop(keys, delay, loopCount, _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task KeyPresserLoop(List<VirtualKeyCode> keys, int delay, int loopCount, CancellationToken cancellationToken)
        {
            for (int i = 0; i < loopCount || loopCount == -1; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var key in keys)
                {
                    _inputSimulator.Keyboard.KeyPress(key);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}
