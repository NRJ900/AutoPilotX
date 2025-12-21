using System;
using System.Windows.Forms;

namespace AutoPilotX
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => 
                Utils.Logger.Error($"Unhandled Exception: {e.ExceptionObject}");
                
            Application.ThreadException += (s, e) =>
                Utils.Logger.Error($"Thread Exception: {e.Exception}");
            
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            try 
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                try {
                    System.IO.File.AppendAllText("crash_dump.txt", $"{DateTime.Now}: {ex}\n");
                } catch { }
                
                MessageBox.Show($"Fatal Error: {ex.Message}", "AutoPilotX Crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
