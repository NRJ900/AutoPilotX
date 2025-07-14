using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoPilotX.Models;
using MouseKeyHook;
using WindowsInput;
using WindowsInput.Native;

namespace AutoPilotX.Services
{
    public class MacroService : IDisposable
    {
        private readonly IKeyboardMouseEvents _globalHook;
        private readonly InputSimulator _inputSimulator;
        private readonly List<MacroEvent> _macroEvents;
        private readonly object _macroEventsLock = new object();
        private readonly Stopwatch _stopwatch;
        private bool _isRecording;
        private CancellationTokenSource _cancellationTokenSource;

        public MacroService()
        {
            _globalHook = Hook.GlobalEvents();
            _inputSimulator = new InputSimulator();
            _macroEvents = new List<MacroEvent>();
            _stopwatch = new Stopwatch();
        }

        public void StartRecording()
        {
            lock (_macroEventsLock)
            {
                _macroEvents.Clear();
            }
            _isRecording = true;
            _stopwatch.Restart();
            _globalHook.MouseMove += OnMouseMove;
            _globalHook.MouseDown += OnMouseDown;
            _globalHook.MouseUp += OnMouseUp;
            _globalHook.KeyDown += OnKeyDown;
            _globalHook.KeyUp += OnKeyUp;
        }

        public void StopRecording()
        {
            _isRecording = false;
            _stopwatch.Stop();
            _globalHook.MouseMove -= OnMouseMove;
            _globalHook.MouseDown -= OnMouseDown;
            _globalHook.MouseUp -= OnMouseUp;
            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.KeyUp -= OnKeyUp;
        }

        public async Task PlayMacro(List<MacroEvent> events)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                long lastTimestamp = 0;
                foreach (var macroEvent in events)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var delay = (int)(macroEvent.Timestamp - lastTimestamp);
                    if (delay > 0)
                        await Task.Delay(delay, token);

                    try
                    {
                        switch (macroEvent.EventType)
                        {
                            case MacroEventType.MouseMove:
                                _inputSimulator.Mouse.MoveMouseTo(macroEvent.X, macroEvent.Y);
                                break;
                            case MacroEventType.MouseDown:
                                _inputSimulator.Mouse.MouseDown((MouseButton)macroEvent.KeyCode);
                                break;
                            case MacroEventType.MouseUp:
                                _inputSimulator.Mouse.MouseUp((MouseButton)macroEvent.KeyCode);
                                break;
                            case MacroEventType.KeyDown:
                                _inputSimulator.Keyboard.KeyDown((VirtualKeyCode)macroEvent.KeyCode);
                                break;
                            case MacroEventType.KeyUp:
                                _inputSimulator.Keyboard.KeyUp((VirtualKeyCode)macroEvent.KeyCode);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore input simulation errors
                    }

                    lastTimestamp = macroEvent.Timestamp;
                }
            }, token);
        }

        public void StopPlayback()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_isRecording) return;
            var abs = System.Windows.Forms.Cursor.Position;
            lock (_macroEventsLock)
            {
                _macroEvents.Add(new MacroEvent
                {
                    EventType = MacroEventType.MouseMove,
                    X = abs.X,
                    Y = abs.Y,
                    Timestamp = _stopwatch.ElapsedMilliseconds
                });
            }
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_isRecording) return;
            lock (_macroEventsLock)
            {
                _macroEvents.Add(new MacroEvent
                {
                    EventType = MacroEventType.MouseDown,
                    KeyCode = (int)e.Button,
                    Timestamp = _stopwatch.ElapsedMilliseconds
                });
            }
        }

        private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!_isRecording) return;
            lock (_macroEventsLock)
            {
                _macroEvents.Add(new MacroEvent
                {
                    EventType = MacroEventType.MouseUp,
                    KeyCode = (int)e.Button,
                    Timestamp = _stopwatch.ElapsedMilliseconds
                });
            }
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!_isRecording) return;
            lock (_macroEventsLock)
            {
                _macroEvents.Add(new MacroEvent
                {
                    EventType = MacroEventType.KeyDown,
                    KeyCode = (int)e.KeyCode,
                    Timestamp = _stopwatch.ElapsedMilliseconds
                });
            }
        }

        private void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!_isRecording) return;
            lock (_macroEventsLock)
            {
                _macroEvents.Add(new MacroEvent
                {
                    EventType = MacroEventType.KeyUp,
                    KeyCode = (int)e.KeyCode,
                    Timestamp = _stopwatch.ElapsedMilliseconds
                });
            }
        }

        public List<MacroEvent> GetMacroEvents()
        {
            lock (_macroEventsLock)
            {
                return new List<MacroEvent>(_macroEvents);
            }
        }

        public void SaveMacro(string filePath, List<MacroEvent> events)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(events);
            System.IO.File.WriteAllText(filePath, json);
        }

        public List<MacroEvent> LoadMacro(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            return System.Text.Json.JsonSerializer.Deserialize<List<MacroEvent>>(json);
        }

        public void Dispose()
        {
            _globalHook.MouseMove -= OnMouseMove;
            _globalHook.MouseDown -= OnMouseDown;
            _globalHook.MouseUp -= OnMouseUp;
            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.KeyUp -= OnKeyUp;
            _globalHook.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
