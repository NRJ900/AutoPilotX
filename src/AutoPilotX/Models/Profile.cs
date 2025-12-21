using System;

namespace AutoPilotX.Models
{
    public class Profile
    {
        public int LastClickInterval { get; set; } = 1000;
        public MouseButton LastMouseButton { get; set; } = MouseButton.Left;
        
        // Add more prefs here
    }
}
