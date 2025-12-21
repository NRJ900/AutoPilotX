using System;

namespace AutoPilotX.Models
{
    public enum ClickType
    {
        Single,
        Double
    }

    public enum RepeatMode
    {
        Infinite,
        Count,
        Duration
    }

    public enum LocationMode
    {
        Current,
        Fixed
    }

    public class AutoClickerSettings
    {
        public int IntervalMs { get; set; } = 100;
        public int RandomIntervalMs { get; set; } = 0; // Jitter
        public MouseButton Button { get; set; } = MouseButton.Left;
        public ClickType ClickType { get; set; } = ClickType.Single;
        
        public RepeatMode RepeatMode { get; set; } = RepeatMode.Infinite;
        public int RepeatCount { get; set; } = 0;
        public int RepeatDurationSeconds { get; set; } = 0;

        public LocationMode LocationMode { get; set; } = LocationMode.Current;
        public int FixedX { get; set; } = 0;
        public int FixedY { get; set; } = 0;
    }
}
