import { useState } from 'react';
import { Settings, Info, Monitor, Moon } from 'lucide-react';
import { clsx } from 'clsx';
import { AppSettings } from '../types';

interface SettingsPanelProps {
    settings: AppSettings;
    onUpdate: (settings: AppSettings) => void;
}

export default function SettingsPanel({ settings, onUpdate }: SettingsPanelProps) {
    const updateSetting = (key: keyof AppSettings, value: any) => {
        onUpdate({ ...settings, [key]: value });
    };

    return (
        <div className="p-8 max-w-4xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-4">
            <h2 className="text-3xl font-bold flex items-center gap-3">
                <Settings className="text-primary" /> Settings
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                {/* Window Settings */}
                <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-6">
                    <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                        <Monitor size={20} className="text-blue-400" /> Window Behavior
                    </h3>

                    <div className="flex items-center justify-between">
                        <div>
                            <div className="font-medium text-white">Always on Top</div>
                            <div className="text-sm text-gray-500">Keep window above other apps</div>
                        </div>
                        <Toggle
                            checked={settings.AlwaysOnTop}
                            onChange={v => updateSetting('AlwaysOnTop', v)}
                        />
                    </div>

                    <div className="flex items-center justify-between">
                        <div>
                            <div className="font-medium text-white">Minimize to Tray</div>
                            <div className="text-sm text-gray-500">Run in background when minimized</div>
                        </div>
                        <Toggle
                            checked={settings.MinimizeToTray}
                            onChange={v => updateSetting('MinimizeToTray', v)}
                        />
                    </div>
                </div>

                {/* Sound */}
                <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-6">
                    <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                        <Moon size={20} className="text-purple-400" /> Sound
                    </h3>

                    <div className="flex items-center justify-between">
                        <div>
                            <div className="font-medium text-white">Sound Effects</div>
                            <div className="text-sm text-gray-500">Play sounds on start/stop</div>
                        </div>
                        <Toggle
                            checked={settings.SoundEffects}
                            onChange={v => updateSetting('SoundEffects', v)}
                        />
                    </div>
                </div>

                {/* About */}
                <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-4 md:col-span-2">
                    <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                        <Info size={20} className="text-green-400" /> About
                    </h3>
                    <div className="text-gray-400 text-sm">
                        <p>AutoPilotX v1.0.0</p>
                        <p>A powerful, hybrid automation tool built with .NET 9 and React.</p>
                        <p className="mt-2 text-xs opacity-50">© 2025 </p>
                    </div>
                </div>

            </div>

            {/* Updates Section */}
            <UpdateCheck />
        </div>
    );
}

function UpdateCheck() {
    const [updateMsg, setUpdateMsg] = useState("");
    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    const checkUpdates = async () => {
        if (!bridge) return;
        setUpdateMsg("Checking...");
        try {
            const json = await bridge.CheckUpdates();
            const info = JSON.parse(json);
            if (info.Url) {
                setUpdateMsg(`New version ${info.Version} available!`);
                if (confirm(`New version ${info.Version} is available. Open download page?`)) {
                    bridge.OpenUpdateUrl(info.Url);
                }
            } else {
                setUpdateMsg("You are up to date.");
            }
        } catch (e) {
            setUpdateMsg("Check failed.");
        }
    };

    return (
        <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-4">
            <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                <span className="text-blue-400">Updates</span>
            </h3>
            <div className="flex items-center gap-4">
                <button
                    onClick={checkUpdates}
                    className="px-4 py-2 bg-blue-600 hover:bg-blue-500 rounded-lg text-sm font-medium transition-colors text-white"
                >
                    Check for Updates
                </button>
                <span className="text-sm text-gray-400">{updateMsg}</span>
            </div>
        </div>
    );
}

function Toggle({ checked, onChange }: { checked: boolean, onChange: (v: boolean) => void }) {
    return (
        <button
            onClick={() => onChange(!checked)}
            className={clsx(
                "w-12 h-6 rounded-full relative transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary/50",
                checked ? "bg-primary" : "bg-white/10"
            )}
        >
            <div className={clsx(
                "w-4 h-4 rounded-full bg-white absolute top-1 transition-all duration-200 shadow-sm",
                checked ? "left-7" : "left-1"
            )} />
        </button>
    );
}
