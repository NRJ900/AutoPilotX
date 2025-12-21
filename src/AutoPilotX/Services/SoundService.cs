using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class SoundService
    {
        private readonly SettingsService _settings;
        private readonly SoundPlayer _startPlayer;
        private readonly SoundPlayer _stopPlayer;

        public SoundService(SettingsService settings)
        {
            _settings = settings;
            
            // TODO: Load actual wav files if they exist
            // For now we will use system beeps or synthetic sounds if possible, 
            // but SoundPlayer requires a stream or file.
            // Let's stick to Console.Beep for a pure code solution without assets,
            // OR checks for "sounds/start.wav".
        }

        public void PlayStart()
        {
            if (!_settings.Settings.SoundEffects) return;

            Task.Run(() => {
                try
                {
                    if (File.Exists("sounds/start.wav"))
                    {
                        using var player = new SoundPlayer("sounds/start.wav");
                        player.Play();
                    }
                    else
                    {
                        // Fallback
                        Console.Beep(1000, 200);
                    }
                }
                catch (Exception ex) 
                {
                    Logger.Error("Failed to play start sound", ex);
                }
            });
        }

        public void PlayStop()
        {
            if (!_settings.Settings.SoundEffects) return;

            Task.Run(() => {
                try
                {
                    if (File.Exists("sounds/stop.wav"))
                    {
                        using var player = new SoundPlayer("sounds/stop.wav");
                        player.Play();
                    }
                    else
                    {
                        // Fallback
                        Console.Beep(800, 200);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to play stop sound", ex);
                }
            });
        }
    }
}
