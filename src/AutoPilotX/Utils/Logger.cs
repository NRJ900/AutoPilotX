using System;
using System.IO;

namespace AutoPilotX.Utils
{
    public static class Logger
    {
        private static readonly string LogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string LogFile = Path.Combine(LogDir, "AutoPilotX.log");
        private static readonly object LockObj = new object();
        private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

        static Logger()
        {
            try
            {
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }
            }
            catch { /* Ignore init errors */ }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message, Exception? ex = null)
        {
            string msg = message;
            if (ex != null)
            {
                msg += $"\nException: {ex}";
            }
            Write("ERROR", msg);
        }
        public static void Debug(string message) => Write("DEBUG", message);

        private static void Write(string level, string message)
        {
            try
            {
                lock (LockObj)
                {
                    CheckRollLog();
                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                    File.AppendAllText(LogFile, line + Environment.NewLine);
                }
            }
            catch
            {
                // Swallow logging errors to prevent app crash
            }
        }

        private static void CheckRollLog()
        {
            try
            {
                if (File.Exists(LogFile) && new FileInfo(LogFile).Length > MaxSizeBytes)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string newName = Path.Combine(LogDir, $"AutoPilotX_{timestamp}.log");
                    File.Move(LogFile, newName);
                }
            }
            catch { }
        }
    }
}
