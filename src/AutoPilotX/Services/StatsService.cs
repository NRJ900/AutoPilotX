using System;
using System.IO;
using System.Text.Json;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class StatsData
    {
        public long TotalClicks { get; set; }
        public long MacrosExecuted { get; set; }
        public double AutoClickerRuntimeSeconds { get; set; }
        public DateTime LastSession { get; set; } = DateTime.Now;
    }

    public class StatsService
    {
        public StatsData Stats { get; private set; } = new StatsData();
        private const string StatsFile = "stats.json";

        private readonly AutoClickerService _autoClicker;
        private readonly MacroService _macro;

        // Runtime temporary tracking
        private DateTime _acStartTime;

        public event EventHandler? StatsUpdated;

        public StatsService(AutoClickerService autoClicker, MacroService macro)
        {
            _autoClicker = autoClicker;
            _macro = macro;

            LoadStats();

            // Subscribe
            _autoClicker.Started += AutoClicker_Started;
            _autoClicker.Stopped += AutoClicker_Stopped;
            _macro.MacroFinished += Macro_MacroFinished;
        }

        private void AutoClicker_Started(object? sender, EventArgs e)
        {
            _acStartTime = DateTime.Now;
        }

        private void AutoClicker_Stopped(object? sender, EventArgs e)
        {
           // Add session clicks
           Stats.TotalClicks += _autoClicker.TotalClicksSession;
           
           // Add duration
           var duration = (DateTime.Now - _acStartTime).TotalSeconds;
           if (duration > 0) Stats.AutoClickerRuntimeSeconds += duration;

           SaveStats();
           StatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void Macro_MacroFinished(object? sender, EventArgs e)
        {
            Stats.MacrosExecuted++;
            SaveStats();
            StatsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void LoadStats()
        {
            if (File.Exists(StatsFile))
            {
                try
                {
                    string json = File.ReadAllText(StatsFile);
                    var loaded = JsonSerializer.Deserialize<StatsData>(json);
                    if (loaded != null) Stats = loaded;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load stats", ex);
                }
            }
        }

        public void SaveStats()
        {
            try
            {
                Stats.LastSession = DateTime.Now;
                string json = JsonSerializer.Serialize(Stats);
                File.WriteAllText(StatsFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save stats", ex);
            }
        }
    }
}
