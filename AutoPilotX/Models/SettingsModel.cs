namespace AutoPilotX.Models
{
    public class SettingsModel
    {
        public bool StartWithWindows { get; set; }
        public string Theme { get; set; } = "Light";
        public string StopKey { get; set; }
        public List<MacroModel> Macros { get; set; } = new List<MacroModel>();
    }
}
