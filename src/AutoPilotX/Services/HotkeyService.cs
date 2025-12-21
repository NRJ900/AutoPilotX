using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class HotkeyDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Keys Key { get; set; }
        public Keys Modifiers { get; set; }
    }

    public class HotkeyService : IDisposable
    {
        private const string HotkeyFile = "hotkeys.json";
        
        // The definitions (What key triggers what ID) - Persisted
        public List<HotkeyDefinition> Definitions { get; private set; } = new List<HotkeyDefinition>();
        
        // The callbacks (What ID triggers what Code) - Runtime
        private readonly Dictionary<string, Action> _callbacks = new Dictionary<string, Action>();

        private IKeyboardMouseEvents _globalHook;

        public event EventHandler? HotkeysChanged;

        public HotkeyService()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += OnKeyDown;
            LoadHotkeys();
        }

        // Called by MainForm/Services to register AVAILABLE actions
        public void RegisterAction(string id, string description, Action callback, Keys defaultKey = Keys.None, Keys defaultModifiers = Keys.None)
        {
            _callbacks[id] = callback;

            // If not already defined (loaded from file), add default
            var existing = Definitions.FirstOrDefault(d => d.Id == id);
            if (existing == null)
            {
                Definitions.Add(new HotkeyDefinition 
                { 
                    Id = id, 
                    Description = description, 
                    Key = defaultKey, 
                    Modifiers = defaultModifiers 
                });
                SaveHotkeys();
            }
            // If exists, we keep the loaded value (user preference wins over default)
            else
            {
                // Ensure description is up to date though
                existing.Description = description;
            }
        }

        // Called by UI Bridge to change binding
        public void SetBinding(string id, Keys key, Keys modifiers)
        {
            var def = Definitions.FirstOrDefault(d => d.Id == id);
            if (def != null)
            {
                def.Key = key;
                def.Modifiers = modifiers;
                SaveHotkeys();
                HotkeysChanged?.Invoke(this, EventArgs.Empty);
                Logger.Info($"Rebound {id} to {modifiers} + {key}");
            }
        }

        public void Unbind(string id)
        {
            SetBinding(id, Keys.None, Keys.None);
        }

        public void UnregisterAction(string id)
        {
            _callbacks.Remove(id);
            var def = Definitions.FirstOrDefault(d => d.Id == id);
            if (def != null)
            {
                Definitions.Remove(def);
                SaveHotkeys();
                HotkeysChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // Find matching definition
            var match = Definitions.FirstOrDefault(d => d.Key == e.KeyCode && d.Modifiers == e.Modifiers);
            if (match != null)
            {
                if (_callbacks.TryGetValue(match.Id, out var action))
                {
                    Logger.Info($"Hotkey triggered: {match.Id}");
                    action.Invoke();
                    e.Handled = true;
                }
            }
        }

        public void LoadHotkeys()
        {
            if (File.Exists(HotkeyFile))
            {
                try
                {
                    string json = File.ReadAllText(HotkeyFile);
                    var loaded = JsonSerializer.Deserialize<List<HotkeyDefinition>>(json);
                    if (loaded != null) Definitions = loaded;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load hotkeys", ex);
                }
            }
            else
            {
                // Preserve default bindings if file missing? 
                // Or clear? HotkeyManager has defaults registered via RegisterAction.
                // If we clear, we lose registered callbacks. 
                // We should NOT clear Definitions, but rather reset to defaults?
                // Actually, RegisterAction adds if missing.
                // So if we switch profile, we might want to reload.
                // Ideally, clear "User Modified" state.
                // For simplicity: just keep current loaded, RegisterAction handles defaults.
            }
        }

        public void SaveHotkeys()
        {
            try
            {
                string json = JsonSerializer.Serialize(Definitions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(HotkeyFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save hotkeys", ex);
            }
        }

        public void Dispose()
        {
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= OnKeyDown;
                _globalHook.Dispose();
            }
        }
    }
}
