using System.Drawing;

namespace AutoPilotX.Utils
{
    public static class ThemeColor
    {
        // Dark grey background (similar to VS Code or Discord)
        public static readonly Color Background = Color.FromArgb(30, 30, 30);
        
        // Slightly lighter for surfaces (panels, listboxes)
        public static readonly Color Surface = Color.FromArgb(45, 45, 48);
        
        // Text colors
        public static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
        public static readonly Color TextSecondary = Color.FromArgb(160, 160, 160);
        
        // Accents (Soft Blue/Purple)
        public static readonly Color Accent = Color.FromArgb(0, 122, 204);
        public static readonly Color AccentHover = Color.FromArgb(28, 151, 234);
        
        // Controls
        public static readonly Color ControlBorder = Color.FromArgb(60, 60, 60);
        public static readonly Color InputBackground = Color.FromArgb(50, 50, 50);
        
        // Status
        public static readonly Color Success = Color.FromArgb(87, 189, 106);
        public static readonly Color Error = Color.FromArgb(244, 75, 86);
        public static readonly Color Warning = Color.FromArgb(255, 193, 7);
    }
}
