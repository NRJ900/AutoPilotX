import { useState } from 'react';
import { Settings, Info, Monitor, Moon, MousePointer2, FileText } from 'lucide-react';
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

                    <div className="flex items-center justify-between mb-2">
                        <span className="text-gray-300">Sound Effects</span>
                        <Toggle
                            checked={settings.SoundEffects}
                            onChange={v => updateSetting('SoundEffects', v)}
                        />
                    </div>

                    {settings.SoundEffects && (
                        <div className="mb-4 px-2">
                            <div className="flex justify-between text-xs text-gray-400 mb-1">
                                <span>Volume</span>
                                <span>{settings.SoundVolume || 50}%</span>
                            </div>
                            <input
                                type="range"
                                min="0"
                                max="100"
                                className="w-full h-2 bg-surface-light rounded-lg appearance-none cursor-pointer accent-primary"
                                value={settings.SoundVolume || 50}
                                onChange={(e) => updateSetting('SoundVolume', parseInt(e.target.value))}
                            />
                        </div>
                    )}
                </div>

                {/* Automation */}
                <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-6">
                    <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                        <MousePointer2 size={20} className="text-orange-400" /> Automation
                    </h3>

                    <div className="flex items-center justify-between">
                        <div>
                            <div className="font-medium text-white flex items-center gap-2">
                                Human-Like Mouse <span className="text-xs bg-primary/20 text-primary px-2 py-0.5 rounded">Beta</span>
                            </div>
                            <div className="text-sm text-gray-500">Smooth, curved cursor movements</div>
                        </div>
                        <Toggle
                            checked={settings.HumanLikeMouseMovement}
                            onChange={v => updateSetting('HumanLikeMouseMovement', v)}
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

                <ReleaseNotes />
            </div>

            {/* Updates Section */}
            <UpdateCheck />
        </div>
    );
}

function ReleaseNotes() {
    return (
        <div className="bg-surface border border-white/10 rounded-xl p-6 space-y-4 md:col-span-2">
            <h3 className="font-bold text-lg flex items-center gap-2 text-white">
                <FileText size={20} className="text-yellow-400" /> Release Notes
            </h3>
            <div className="space-y-6">
                {/* Latest */}
                <div className="relative pl-4 border-l-2 border-primary/30">
                    <div className="flex items-center gap-2 mb-1">
                        <span className="font-bold text-white">v1.2.0</span>
                        <span className="text-xs bg-primary/20 text-primary px-2 py-0.5 rounded">Current</span>
                    </div>
                    <ul className="text-sm text-gray-400 space-y-1">
                        <li>• <span className="text-gray-300">Human-Like Mouse</span>: Smooth, natural cursor curves.</li>
                        <li>• <span className="text-gray-300">Mini-Overlay Mode</span>: Compact widget for multitasking.</li>
                        <li>• <span className="text-gray-300">Sound Effects</span>: Audio feedback with volume control.</li>
                        <li>• <span className="text-gray-300">Visual Polish</span>: Enhanced UI consistency and animations.</li>
                    </ul>
                </div>

                {/* Upcoming */}
                <div className="relative pl-4 border-l-2 border-gray-700">
                    <div className="flex items-center gap-2 mb-1">
                        <span className="font-medium text-gray-400">Upcoming Features</span>
                    </div>
                    <ul className="text-sm text-gray-500 space-y-1">
                        <li>• Smart Vision (Color Detection)</li>
                        <li>• Advanced Macro Logic (If/Else, Loops)</li>
                        <li>• Cloud Sync & Scripting</li>
                    </ul>
                </div>
            </div>
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
                "w-11 h-6 rounded-full relative transition-colors duration-200 ease-in-out focus:outline-none",
                checked ? "bg-primary" : "bg-white/10"
            )}
        >
            <div className={clsx(
                "w-5 h-5 rounded-full bg-white absolute top-0.5 left-0.5 transition-transform duration-200 shadow-sm",
                checked ? "translate-x-full" : "translate-x-0"
            )} />
        </button>
    );
}
