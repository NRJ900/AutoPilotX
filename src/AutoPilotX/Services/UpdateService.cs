using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class UpdateService
    {
        private const string UpdateJsonUrl = "https://raw.githubusercontent.com/NRJ900/AutoPilotX/main/version.json";
        private const string CurrentVersion = "1.0.0";

        public async Task<UpdateInfo> CheckForUpdates()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var json = await client.GetStringAsync(UpdateJsonUrl);
                var remote = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (remote != null && IsNewer(remote.Version))
                {
                    return remote;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check for updates", ex);
            }
            return new UpdateInfo { Version = CurrentVersion, Url = "" }; // No update
        }

        private bool IsNewer(string remoteVersion)
        {
            var v1 = Version.Parse(CurrentVersion);
            var v2 = Version.Parse(remoteVersion);
            return v2 > v1;
        }

        public void OpenDownloadPage(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                 Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; } = "1.0.0";
        public string Url { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
