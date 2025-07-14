using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InputSimulator;
using InputSimulator.Native;

namespace AutoPilotX.Services
{
    public class KeyPresserService : IDisposable
    {
        private readonly IInputSimulator _inputSimulator = new InputSimulator();
        private CancellationTokenSource _cancellationTokenSource;

        public async Task StartPressing(IEnumerable<VirtualKeyCode> keys, int delay)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var key in keys)
                    {
                        try
                        {
                            _inputSimulator.Keyboard.KeyPress(key);
                        }
                        catch (Exception)
                        {
                            // Ignore input simulation errors
                        }
                        await Task.Delay(delay, token);
                    }
                }
            }, token);
        }

        public void StopPressing()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}
