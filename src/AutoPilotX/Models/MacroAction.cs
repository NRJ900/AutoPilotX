using System;
using System.Text.Json.Serialization;

namespace AutoPilotX.Models
{
    public enum MacroActionType
    {
        MouseMove,
        MouseClick,
        MouseScroll,
        KeyDown,
        KeyUp,
        Delay
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public class MacroAction
    {
        public MacroActionType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public MouseButton Button { get; set; }
        public string Key { get; set; } = string.Empty;
        public int ScrollAmount { get; set; }
        public int DelayMs { get; set; }

        // Helper factories for readable code
        public static MacroAction Move(int x, int y) => new MacroAction
        {
            Type = MacroActionType.MouseMove,
            X = x,
            Y = y
        };

        public static MacroAction Click(int x, int y, MouseButton button) => new MacroAction
        {
            Type = MacroActionType.MouseClick,
            X = x,
            Y = y,
            Button = button
        };

        public static MacroAction KeyDown(string key) => new MacroAction
        {
            Type = MacroActionType.KeyDown,
            Key = key
        };

        public static MacroAction KeyUp(string key) => new MacroAction
        {
            Type = MacroActionType.KeyUp,
            Key = key
        };

        public static MacroAction Delay(int ms) => new MacroAction
        {
            Type = MacroActionType.Delay,
            DelayMs = ms
        };

        public static MacroAction Scroll(int amount) => new MacroAction
        {
             Type = MacroActionType.MouseScroll,
             ScrollAmount = amount
        };
    }
}
