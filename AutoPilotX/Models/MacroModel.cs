using System.Collections.Generic;

namespace AutoPilotX.Models
{
    public class MacroModel
    {
        public string Name { get; set; }
        public List<MacroEvent> Events { get; set; } = new List<MacroEvent>();
    }

    public class MacroEvent
    {
        public MacroEventType EventType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int KeyCode { get; set; }
        public int Delay { get; set; }
    }

    public enum MacroEventType
    {
        MouseMove,
        MouseClick,
        KeyPress
    }
}
