using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutoPilotX.Models;

namespace AutoPilotX.Services
{
    public interface IInputSimulator
    {
        void MoveMouse(int x, int y);
        void Click(MouseButton button);
        void KeyDown(string key);
        void KeyUp(string key);
        void DoubleClick(MouseButton button);
        void Scroll(int amount);
    }

    public class InputSimulatorWrapper : IInputSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        // Mouse flags
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        // Keyboard flags
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        public void MoveMouse(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public void Scroll(int amount)
        {
             mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)amount, 0);
        }

        public void Click(MouseButton button)
        {
            uint down = 0;
            uint up = 0;

            switch (button)
            {
                case MouseButton.Left:
                    down = MOUSEEVENTF_LEFTDOWN;
                    up = MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButton.Right:
                    down = MOUSEEVENTF_RIGHTDOWN;
                    up = MOUSEEVENTF_RIGHTUP;
                    break;
                case MouseButton.Middle:
                    down = MOUSEEVENTF_MIDDLEDOWN;
                    up = MOUSEEVENTF_MIDDLEUP;
                    break;
            }

            mouse_event(down, 0, 0, 0, 0);
            mouse_event(up, 0, 0, 0, 0);
        }

        public void DoubleClick(MouseButton button)
        {
            Click(button);
            System.Threading.Thread.Sleep(50); // Small delay for OS to register double click
            Click(button);
        }

        public void KeyDown(string key)
        {
            if (Enum.TryParse(key, out Keys k))
            {
                keybd_event((byte)k, 0, 0, 0);
            }
        }

        public void KeyUp(string key)
        {
            if (Enum.TryParse(key, out Keys k))
            {
                keybd_event((byte)k, 0, KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
