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
            PlayBeep(500, 100);
        }

        public void PlayStop()
        {
            if (!_settings.Settings.SoundEffects) return;
            PlayBeep(400, 100);
        }

        private void PlayBeep(int frequency, int durationMs)
        {
            Task.Run(() => {
                try
                {
                    // Generate WAV in memory
                    double volume = (_settings.Settings.SoundVolume / 100.0);
                    if (volume <= 0) return; // Mute

                    using var stream = GenerateBeepStream(frequency, durationMs, volume);
                    using var player = new SoundPlayer(stream);
                    player.Play();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to play sound", ex);
                }
            });
        }

        private MemoryStream GenerateBeepStream(int frequency, int durationMs, double volume)
        {
            var mStrm = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mStrm);

            int sampleRate = 44100;
            short channels = 1;
            short bitsPerSample = 16;
            int samples = sampleRate * durationMs / 1000;
            int bytesPerSample = bitsPerSample / 8;
            int dataLength = samples * bytesPerSample * channels;

            // Header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Chunk size
            writer.Write((short)1); // PCM
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bytesPerSample); // Byte rate
            writer.Write((short)(channels * bytesPerSample)); // Block align
            writer.Write(bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);

            // Data
            double amplitude = volume * 32760; // Max short
            double theta = frequency * 2 * Math.PI / sampleRate;

            for (int i = 0; i < samples; i++)
            {
                short sample = (short)(amplitude * Math.Sin(theta * i));
                writer.Write(sample);
            }

            writer.Flush();
            mStrm.Position = 0;
            return mStrm;
        }
    }
}
