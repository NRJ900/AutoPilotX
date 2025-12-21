using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using AutoPilotX.Services;
using AutoPilotX.Utils;
using AutoPilotX.Models;
using System.Text.Json;

namespace AutoPilotX
{
    public partial class MainForm : Form
    {
        private WebView2? _webView;
        private Bridge? _bridge;

        // Services
        private readonly AutoClickerService _autoClicker;
        private readonly HotkeyService _hotkey;
        private readonly MacroService _macro;
        private readonly StatsService _statsService;
        private readonly ProfileService _profileService;
        private readonly SettingsService _settingsService;
        private NotifyIcon? _notifyIcon;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            
            // Restore Production Styling
            ThemeUtils.UseImmersiveDarkMode(this.Handle, true);
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Restore Services
            _settingsService = new SettingsService();
            var soundService = new SoundService(_settingsService); // New
            
            _autoClicker = new AutoClickerService(soundService);
            _hotkey = new HotkeyService();
            _macro = new MacroService(soundService);
            
            _statsService = new StatsService(_autoClicker, _macro);
            _profileService = new ProfileService(_macro, _hotkey, _statsService, _settingsService);
            
            // Apply Initial Settings
            ApplySettings();
            _settingsService.SettingsChanged += (s, args) => ApplySettings();
        }

        private void ApplySettings()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ApplySettings));
                return;
            }

            var settings = _settingsService.Settings;
            this.TopMost = settings.AlwaysOnTop;
            
            // Tray Logic
            if (_notifyIcon == null)
            {
                _notifyIcon = new NotifyIcon
                {
                    Visible = false,
                    Text = "AutoPilotX"
                };

                // Try Load Icon
                try 
                { 
                    if (System.IO.File.Exists("app.ico")) 
                    {
                        var icon = new Icon("app.ico");
                        _notifyIcon.Icon = icon;
                        this.Icon = icon; // Set Window Icon too
                    }
                    else
                    {
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                } 
                catch { _notifyIcon.Icon = SystemIcons.Application; }
                
                _notifyIcon.DoubleClick += (s, args) => 
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    _notifyIcon.Visible = false;
                };

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Open", null, (s, args) => {
                     this.Show();
                     this.WindowState = FormWindowState.Normal;
                     _notifyIcon.Visible = false;
                });
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Exit", null, (s, args) => Application.Exit());
                _notifyIcon.ContextMenuStrip = contextMenu;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Minimized && _settingsService.Settings.MinimizeToTray)
            {
                this.Hide();
                if (_notifyIcon != null) _notifyIcon.Visible = true;
            }
        }

        private void ToggleAutoClicker()
        {
            if (_autoClicker.IsRunning)
                _autoClicker.Stop();
            else
                _autoClicker.Start(_autoClicker.Settings);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _webView = new WebView2();
                _webView.Dock = DockStyle.Fill;
                _webView.DefaultBackgroundColor = Color.FromArgb(30, 30, 30);
                this.Controls.Add(_webView);

                var env = await CoreWebView2Environment.CreateAsync(null, null, null);
                await _webView.EnsureCoreWebView2Async(env);
                
                _webView.NavigationCompleted += (s, args) =>
                {
                    if (!args.IsSuccess)
                    {
                        MessageBox.Show($"Navigation failed: {args.WebErrorStatus}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                _bridge = new Bridge(_autoClicker, _hotkey, _macro, _statsService, _profileService, _settingsService, EmitEvent);
                _webView.CoreWebView2.AddHostObjectToScript("bridge", _bridge);
                
                // Enable DevTools
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

                // Determine Source
#if DEBUG
                _webView.Source = new Uri("http://localhost:5173");
#else
                // Production: Map virtual host to local directory
                string distPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dist");
                _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "autopilotx.local", 
                    distPath, 
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
                
                _webView.Source = new Uri("http://autopilotx.local/index.html");
#endif 
                
                // Register Global Logic
                _hotkey.RegisterAction("ToggleAutoClicker", "Toggle AutoClicker", ToggleAutoClicker, Keys.F6); 
                _hotkey.RegisterAction("ToggleRecording", "Toggle Recording", () => _macro?.ToggleRecording(), Keys.F9); 

                // Sync Macros
                _macro.MacrosChanged += (s, args) => SyncMacroHotkeys();
                SyncMacroHotkeys();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start Error: {ex.Message}", "AutoPilotX Error");
            }
        }

        private void SyncMacroHotkeys()
        {
            // Add new/update
            foreach (var m in _macro.Macros)
            {
                string id = $"Macro:{m.Name}";
                _hotkey.RegisterAction(id, $"Play Macro '{m.Name}'", () => {
                    if (_macro.IsPlaying)
                    {
                        if (_macro.CurrentPlayingMacro?.Name == m.Name) // Check Name or Id
                        {
                            _macro.StopPlayback();
                            return;
                        }
                        else
                        {
                            // If different macro, stop first?
                            _macro.StopPlayback();
                             // TODO: Wait for stop? For now just stop the other one.
                             // Starting a new one immediately might fail on "Macro already playing" check 
                             // if the async task hasn't finished cleanup.
                             // For safety, let's just return to avoid exception, effectively "Stop override".
                             return;
                        }
                    }

                    // Start
                    Task.Run(() => _macro.StartMacro(m));
                });
            }

            // Remove deleted
            var stale = _hotkey.Definitions
                .Where(d => d.Id.StartsWith("Macro:") && !_macro.Macros.Any(m => $"Macro:{m.Name}" == d.Id))
                .ToList();
            
            foreach (var s in stale)
            {
                _hotkey.UnregisterAction(s.Id);
            }
        }

        private void EmitEvent(string name, object data)
        {
            if (_webView != null && _webView.CoreWebView2 != null)
            {
                try
                {
                    string json = JsonSerializer.Serialize(data);
                    this.Invoke((MethodInvoker)delegate {
                        _webView.CoreWebView2.ExecuteScriptAsync($"window.dispatchEvent(new CustomEvent('{name}', {{ detail: {json} }}));");
                    });
                }
                catch { /* Ignore updates if view is closing */ }
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Text = "AutoPilotX";
        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _autoClicker?.Dispose();
                _hotkey?.Dispose();
                _macro?.Dispose();
                _notifyIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
