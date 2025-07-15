using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AutoPilotX.Models;
using Gma.System.MouseKeyHook;
using WindowsInput;

namespace AutoPilotX.Services
{
    public class MacroService
    {
        private readonly IKeyboardMouseEvents _globalHook;
        private readonly IInputSimulator _inputSimulator;
        private List<MacroEvent> _events;
        private Stopwatch _stopwatch;
        private bool _isRecording;

        public MacroService(IInputSimulator inputSimulator)
        {
            _inputSimulator = inputSimulator;
            _globalHook = Hook.GlobalEvents();
            _events = new List<MacroEvent>();
            _stopwatch = new Stopwatch();
        }

        public void StartRecording()
        {
            _events.Clear();
            _isRecording = true;
            _stopwatch.Start();
            _globalHook.KeyDown += OnKeyDown;
            _globalHook.MouseDown += OnMouseDown;
        }

        public MacroModel StopRecording()
        {
            _stopwatch.Stop();
            _isRecording = false;
            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.MouseDown -= OnMouseDown;

            return new MacroModel { Events = _events };
        }

        public void PlayMacro(MacroModel macro)
        {
            Task.Run(() =>
            {
                foreach (var e in macro.Events)
                {
                    Thread.Sleep(e.Delay);
                    switch (e.EventType)
                    {
                        case MacroEventType.KeyPress:
                            _inputSimulator.Keyboard.KeyPress((WindowsInput.Native.VirtualKeyCode)e.KeyCode);
                            break;
                        case MacroEventType.MouseClick:
                            _inputSimulator.Mouse.GoTo(e.X, e.Y);
                            _inputSimulator.Mouse.LeftButtonClick();
                            break;
                    }
                }
            });
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (_isRecording)
            {
                _events.Add(new MacroEvent
                {
                    EventType = MacroEventType.KeyPress,
                    KeyCode = (int)e.KeyCode,
                    Delay = (int)_stopwatch.ElapsedMilliseconds
                });
                _stopwatch.Restart();
            }
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_isRecording)
            {
                _events.Add(new MacroEvent
                {
                    EventType = MacroEventType.MouseClick,
                    X = e.X,
                    Y = e.Y,
                    Delay = (int)_stopwatch.ElapsedMilliseconds
                });
                _stopwatch.Restart();
            }
        }
    }
}
