namespace AutoPilotX.Models
{
    public enum MacroEventType
    {
        MouseMove,
        MouseDown,
        MouseUp,
        KeyDown,
        KeyUp
    }

    public class MacroEvent
    {
        public MacroEventType EventType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int KeyCode { get; set; }
        public long Timestamp { get; set; }
    }
}
