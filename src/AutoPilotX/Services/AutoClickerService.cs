using System;
using System.Threading;
using System.Threading.Tasks;
using AutoPilotX.Models;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class AutoClickerService : IDisposable
    {
        public bool IsRunning { get; private set; }
        public AutoClickerSettings Settings { get; private set; } = new AutoClickerSettings();
        public long TotalClicksSession { get; private set; }

        public event EventHandler? Started;
        public event EventHandler? Stopped;

        private CancellationTokenSource? _cts;
        private readonly IInputSimulator _inputSim;
        private readonly Random _random = new Random();
        private readonly SoundService? _soundService;

        public AutoClickerService(SoundService? soundService = null)
        {
            _inputSim = new InputSimulatorWrapper();
            _soundService = soundService;
        }

        public void Start(AutoClickerSettings settings)
        {
            if (IsRunning) return;

            Settings = settings;
            IsRunning = true;
            TotalClicksSession = 0;
            _cts = new CancellationTokenSource();
            Started?.Invoke(this, EventArgs.Empty);
            _soundService?.PlayStart();

            Task.Run(async () => await RunLoop(_cts.Token));
        }

        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            _cts?.Cancel();
            Stopped?.Invoke(this, EventArgs.Empty);
            _soundService?.PlayStop();
        }

        private async Task RunLoop(CancellationToken token)
        {
            Logger.Info($"AutoClicker started. Mode: {Settings.RepeatMode}");
            try
            {
                // Safety Delay
                await Task.Delay(1000, token);

                long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int clicksPerformed = 0;

                while (!token.IsCancellationRequested)
                {
                    // Check Limits
                    if (Settings.RepeatMode == RepeatMode.Count && clicksPerformed >= Settings.RepeatCount) break;
                    if (Settings.RepeatMode == RepeatMode.Duration)
                    {
                        long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
                        if (elapsed >= Settings.RepeatDurationSeconds * 1000) break;
                    }

                    // Move if fixed
                    if (Settings.LocationMode == LocationMode.Fixed)
                    {
                        _inputSim.MoveMouse(Settings.FixedX, Settings.FixedY);
                    }

                    // Click
                    if (Settings.ClickType == ClickType.Double)
                        _inputSim.DoubleClick(Settings.Button);
                    else
                        _inputSim.Click(Settings.Button);

                    clicksPerformed++;
                    TotalClicksSession++;

                    // Interval with Randomness
                    int delay = Settings.IntervalMs;
                    if (Settings.RandomIntervalMs > 0)
                    {
                        int jitter = _random.Next(-Settings.RandomIntervalMs, Settings.RandomIntervalMs + 1);
                        delay += jitter;
                    }
                    if (delay < 1) delay = 1; // Minimum safety

                    await Task.Delay(delay, token);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Logger.Error("AutoClicker loop error", ex);
            }
            finally
            {
                IsRunning = false;
                Logger.Info("AutoClicker loop ended.");
                // Ensure UI knows we stopped if it wasn't a manual stop
                if (!_cts.IsCancellationRequested)
                {
                     Stopped?.Invoke(this, EventArgs.Empty); 
                }
            }
        }
        
        // Pick Location Helper
        private TaskCompletionSource<(int x, int y)>? _pickTcs;
        private Gma.System.MouseKeyHook.IKeyboardMouseEvents? _pickHook;

        public async Task<(int x, int y)> PickLocation()
        {
            if (_pickHook != null) return (0, 0); // Already picking

            _pickTcs = new TaskCompletionSource<(int x, int y)>();
            
            // Run on UI thread context if possible, but for hook generic is fine
            _pickHook = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            _pickHook.MouseDownExt += PickHook_MouseDown;
            
            // Timeout 10s
            var timeoutTask = Task.Delay(10000);
            var resultTask = _pickTcs.Task;
            
            var completed = await Task.WhenAny(resultTask, timeoutTask);
            
            CleanupPickHook();

            if (completed == resultTask) return await resultTask;
            return (0, 0); // Timeout
        }

        private void CleanupPickHook()
        {
             if (_pickHook != null)
            {
                _pickHook.MouseDownExt -= PickHook_MouseDown;
                _pickHook.Dispose();
                _pickHook = null;
            }
        }

        private void PickHook_MouseDown(object? sender, Gma.System.MouseKeyHook.MouseEventExtArgs e)
        {
            e.Handled = true; // Eat the click
            _pickTcs?.TrySetResult((e.X, e.Y));
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            CleanupPickHook();
        }
    }
}
