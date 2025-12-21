using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoPilotX.Models;
using AutoPilotX.Utils;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;

namespace AutoPilotX.Services
{
    public class MacroService : IDisposable
    {
        public List<Macro> Macros { get; private set; } = new List<Macro>();
        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;
        public Macro? CurrentPlayingMacro { get; private set; }
        
        public event EventHandler? MacrosChanged;
        public event EventHandler? MacroFinished;

        private readonly IInputSimulator _inputSim;
        private readonly SoundService? _soundService;
        private readonly MouseMovementService? _movementService;
        
        public MacroService(SoundService? soundService = null, MouseMovementService? movementService = null)
        {
            _inputSim = new InputSimulatorWrapper();
            _soundService = soundService;
            _movementService = movementService;
            LoadMacros();
            MacrosChanged += (s, e) => SaveMacros();
        }

        public void AddMacro(Macro macro)
        {
            Macros.Add(macro);
            MacrosChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool RemoveMacro(Macro macro)
        {
            bool removed = Macros.Remove(macro);
            if (removed)
            {
                MacrosChanged?.Invoke(this, EventArgs.Empty);
            }
            return removed;
        }

        public void UpdateMacro(Macro newMacro)
        {
            var existing = Macros.Find(m => m.Id == newMacro.Id || m.Name == newMacro.Name);
            if (existing != null)
            {
                int index = Macros.IndexOf(existing);
                Macros[index] = newMacro;
                MacrosChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task StartMacro(Macro macro)
        {
            if (_isPlaying) throw new InvalidOperationException("Macro already playing");

            _isPlaying = true;
            CurrentPlayingMacro = macro;
            Logger.Info($"Starting macro: {macro.Name}");
            _soundService?.PlayStart();

            // Loop Logic
            int iterations = 1;
            if (macro.RepeatMode == RepeatMode.Count) iterations = macro.RepeatCount;
            else if (macro.RepeatMode == RepeatMode.Infinite) iterations = int.MaxValue;

            // Relative Logic
            int offsetX = 0;
            int offsetY = 0;
            if (macro.IsRelative)
            {
                var currentPos = Cursor.Position;
                offsetX = currentPos.X - macro.OriginalReferenceX;
                offsetY = currentPos.Y - macro.OriginalReferenceY;
                Logger.Info($"Relative Macro: Offset by ({offsetX}, {offsetY})");
            }

            try
            {
                // Startup delay to allow focus switch
                await Task.Delay(500);

                for (int i = 0; i < iterations; i++)
                {
                    if (!_isPlaying) break; // Check cancel

                    foreach (var action in macro.Actions)
                    {
                        if (!_isPlaying) break;

                        // Speed Logic
                        int finalDelay = (int)(action.DelayMs * (1.0 / macro.SpeedMultiplier));
                        if (finalDelay < 0) finalDelay = 0;

                        // Apply delay first or after? 
                        // Our recording logic puts delay BEFORE action usually (via explicit delay actions).
                        // If type is Delay, just wait.
                        
                        switch (action.Type)
                        {
                            case MacroActionType.MouseMove:
                                if (_movementService != null)
                                    _movementService.MoveMouse(action.X + offsetX, action.Y + offsetY);
                                else
                                    _inputSim.MoveMouse(action.X + offsetX, action.Y + offsetY);
                                break;
                            case MacroActionType.MouseClick:
                                // Click likely relies on current position if X/Y not specified in action?
                                // If X,Y provided, move there first.
                                if (_movementService != null)
                                     _movementService.MoveMouse(action.X + offsetX, action.Y + offsetY);
                                else 
                                     _inputSim.MoveMouse(action.X + offsetX, action.Y + offsetY);

                                _inputSim.Click(action.Button);
                                break;
                            case MacroActionType.KeyDown:
                                _inputSim.KeyDown(action.Key);
                                break;
                            case MacroActionType.KeyUp:
                                _inputSim.KeyUp(action.Key);
                                break;
                            case MacroActionType.Delay:
                                 // Just a placeholder for delay, handled below
                                 break;
                            case MacroActionType.MouseScroll:
                                 _inputSim.Scroll(action.ScrollAmount);
                                 break;
                        }
                        
                        // Always wait for the action's delay (scaled by speed)
                        if (finalDelay > 0) 
                        {
                            await Task.Delay(finalDelay);
                        }
                        
                        // Small safe delay?
                        if (macro.SpeedMultiplier > 10) await Task.Delay(1); 
                    }
                    
                    // Allow UI update/check between loops
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error playing macro {macro.Name}", ex);
                throw;
            }
            finally
            {
                _isPlaying = false;
                CurrentPlayingMacro = null;
                Logger.Info($"Finished macro: {macro.Name}");
                ReleaseModifiers(); // Ensure keys aren't stuck
                MacroFinished?.Invoke(this, EventArgs.Empty);
                _soundService?.PlayStop();
            }
        }

        public void StopPlayback()
        {
            if (_isPlaying)
            {
                _isPlaying = false; // Flag loop to break
                // The finally block in StartMacro will handle cleanup
            }
        }

        private void ReleaseModifiers()
        {
            try
            {
                // Safety: Force release common modifiers to prevent stuck keys
                _inputSim.KeyUp("LWin");
                _inputSim.KeyUp("RWin");
                _inputSim.KeyUp("LControlKey");
                _inputSim.KeyUp("RControlKey");
                _inputSim.KeyUp("LShiftKey");
                _inputSim.KeyUp("RShiftKey");
                _inputSim.KeyUp("LMenu"); // Alt
                _inputSim.KeyUp("RMenu"); // Alt
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to release modifiers", ex);
            }
        }

        // Persistence
        private const string MacroFile = "macros.json";

        public void LoadMacros()
        {
            if (System.IO.File.Exists(MacroFile))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(MacroFile);
                    var loaded = System.Text.Json.JsonSerializer.Deserialize<List<Macro>>(json);
                    if (loaded != null)
                    {
                        Macros = loaded;
                        MacrosChanged?.Invoke(this, EventArgs.Empty); // Ensure UI updates on reload
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load macros", ex);
                }
            }
            else
            {
                Macros.Clear(); // Clear if file missing (e.g. new profile)
                MacrosChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SaveMacros()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(Macros);
                System.IO.File.WriteAllText(MacroFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save macros", ex);
            }
        }

        // Recording State
        private IKeyboardMouseEvents? _globalHook;
        private Macro? _currentRecordingMacro;
        private bool _isRecording;
        private bool _isReadyToRecord;
        private bool _isCountingDown;
        private long _lastActionTime;
        
        // Options
        private bool _recordKeys;
        private bool _recordMouse;

        public event EventHandler<string>? RecordingStatusChanged;

        public void PrepareRecording(string name, bool recordKeys, bool recordMouse)
        {
            if (_isPlaying)
            {
                // Force stop playing if user wants to record
                _isPlaying = false;
                Thread.Sleep(50); // Give loop a moment to exit
            }

            if (_isRecording || _isCountingDown) throw new InvalidOperationException("Busy");

            _currentRecordingMacro = new Macro(name);
            _recordKeys = recordKeys;
            _recordMouse = recordMouse;
            _isReadyToRecord = true;
            _isRecording = false;
            _isCountingDown = false;
            
            RecordingStatusChanged?.Invoke(this, "Waiting to start...");
            Logger.Info($"Prepared recording: {name}. Waiting to start...");
        }

        public void ToggleRecording()
        {
            if (_isReadyToRecord && !_isRecording && !_isCountingDown)
            {
                _isReadyToRecord = false;
                _isCountingDown = true;
                _countdownSeconds = 5;
                
                // Use a Timer to stay on the main thread
                _countdownTimer = new System.Windows.Forms.Timer();
                _countdownTimer.Interval = 1000;
                _countdownTimer.Tick += (s, e) => 
                {
                    if (_countdownSeconds > 0)
                    {
                        RecordingStatusChanged?.Invoke(this, $"Starting in {_countdownSeconds}...");
                        _countdownSeconds--;
                    }
                    else
                    {
                        if (_countdownTimer != null) 
                        {
                             _countdownTimer.Stop();
                             _countdownTimer.Dispose();
                             _countdownTimer = null;
                        }
                        _isCountingDown = false;
                        StartRecordingInternal();
                    }
                };
                _countdownTimer.Start();
                RecordingStatusChanged?.Invoke(this, $"Starting in 5...");
            }
            else if (_isCountingDown)
            {
                CancelRecording();
            }
            else if (_isRecording)
            {
                StopRecordingInternal();
            }
        }

        private System.Windows.Forms.Timer? _countdownTimer;
        private int _countdownSeconds;

        private void StartRecordingInternal()
        {
            _isRecording = true;
            _lastActionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            // Capture Start Position for Relative Mode
            if (_currentRecordingMacro != null)
            {
                var startPos = Cursor.Position;
                _currentRecordingMacro.OriginalReferenceX = startPos.X;
                _currentRecordingMacro.OriginalReferenceY = startPos.Y;
            }

            // Setup hooks
            _globalHook = Hook.GlobalEvents();
            if (_recordMouse)
            {
                _globalHook.MouseMoveExt += GlobalHook_MouseMoveExt;
                _globalHook.MouseDownExt += GlobalHook_MouseDownExt;
                _globalHook.MouseWheelExt += GlobalHook_MouseWheelExt;
            }
            if (_recordKeys)
            {
                _globalHook.KeyDown += GlobalHook_KeyDown;
                _globalHook.KeyUp += GlobalHook_KeyUp;
            }
            
            RecordingStatusChanged?.Invoke(this, "Recording...");
            Logger.Info($"Started recording.");
        }

        private void StopRecordingInternal()
        {
            _isRecording = false;
            if (_globalHook != null)
            {
                if (_recordMouse)
                {
                    _globalHook.MouseMoveExt -= GlobalHook_MouseMoveExt;
                    _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
                    _globalHook.MouseWheelExt -= GlobalHook_MouseWheelExt;
                }
                if (_recordKeys)
                {
                    _globalHook.KeyDown -= GlobalHook_KeyDown;
                    _globalHook.KeyUp -= GlobalHook_KeyUp;
                }
                _globalHook.Dispose();
                _globalHook = null;
            }
            
            Logger.Info($"Stopped recording macro. Actions: {_currentRecordingMacro?.Actions.Count ?? 0}");
            
            var recordedMacro = _currentRecordingMacro;
            if (recordedMacro != null && recordedMacro.Actions.Count > 0)
            {
                AddMacro(recordedMacro);
            }
            
            _currentRecordingMacro = null;
            RecordingStatusChanged?.Invoke(this, "Stopped");
        }

        // Exposed for Bridge to cancel
        public void CancelRecording()
        {
            _isReadyToRecord = false;
            _isRecording = false;
            _isCountingDown = false;
            
            if (_countdownTimer != null)
            {
                _countdownTimer.Stop();
                _countdownTimer.Dispose();
                _countdownTimer = null;
            }

            if (_globalHook != null)
            {
                _globalHook.Dispose();
                _globalHook = null;
            }
            _currentRecordingMacro = null;
            RecordingStatusChanged?.Invoke(this, "Cancelled");
        }
        
        // Old methods shimmed or removed
        public void StartRecording(string name) => PrepareRecording(name, true, true); // Legacy
        public Macro StopRecording() { StopRecordingInternal(); return new Macro(); } // Legacy

        // Hook Handlers
        private void GlobalHook_KeyUp(object? sender, KeyEventArgs e)
        {
           if (e.KeyCode == Keys.F9) return; // Ignore toggle key
           AddRecordedAction(MacroAction.KeyUp(e.KeyCode.ToString()));
        }

        private void GlobalHook_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9) return; // Ignore toggle key
            AddRecordedAction(MacroAction.KeyDown(e.KeyCode.ToString()));
        }

        private void GlobalHook_MouseDownExt(object? sender, MouseEventExtArgs e)
        {
             MouseButton? btn = e.Button switch
             {
                 MouseButtons.Left => MouseButton.Left,
                 MouseButtons.Right => MouseButton.Right,
                 MouseButtons.Middle => MouseButton.Middle,
                 _ => null
             };

             if (btn.HasValue)
             {
                 AddRecordedAction(MacroAction.Click(e.X, e.Y, btn.Value));
             }
        }

        private void GlobalHook_MouseMoveExt(object? sender, MouseEventExtArgs e)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - _lastActionTime > 50) // Throttle 50ms
            {
                 AddRecordedAction(MacroAction.Move(e.X, e.Y));
            }
        }

        private void GlobalHook_MouseWheelExt(object? sender, MouseEventExtArgs e)
        {
             // e.Delta is scroll amount
             if (e.Delta != 0)
             {
                 AddRecordedAction(MacroAction.Scroll(e.Delta));
             }
        }

        private void AddRecordedAction(MacroAction action)
        {
            if (!_isRecording || _currentRecordingMacro == null) return;
            
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int delay = (int)(now - _lastActionTime);
            if (delay < 0) delay = 0;
            
            // Add a delay action if significant time passed
            if (delay > 10)
            {
                 _currentRecordingMacro.AddAction(MacroAction.Delay(delay));
            }

            _currentRecordingMacro.AddAction(action);
            _lastActionTime = now;
        }

        public void Dispose()
        {
            if (_globalHook != null)
            {
                 _globalHook.Dispose();
            }
        }
    }
}
