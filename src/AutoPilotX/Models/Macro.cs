using System;
using System.Collections.Generic;

namespace AutoPilotX.Models
{
    public class Macro
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<MacroAction> Actions { get; set; } = new List<MacroAction>();
        
        // Advanced Features
        public RepeatMode RepeatMode { get; set; } = RepeatMode.Infinite; // Re-use enum from AutoClickerSettings
        public int RepeatCount { get; set; } = 0; // 0 = Infinite if mode is Infinite
        public double SpeedMultiplier { get; set; } = 1.0;
        
        public bool IsRelative { get; set; } = false;
        public int OriginalReferenceX { get; set; } = 0;
        public int OriginalReferenceY { get; set; } = 0;

        public Macro() { }

        public Macro(string name)
        {
            Name = name;
        }

        public void AddAction(MacroAction action)
        {
            Actions.Add(action);
        }

        public override string ToString()
        {
            return $"{Name} ({Actions.Count} actions)";
        }
    }
}
