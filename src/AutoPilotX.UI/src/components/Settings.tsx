import { useState } from 'react';
import { createPortal } from 'react-dom';
import { Settings, Info, Monitor, Moon, MousePointer2, FileText, X, ChevronRight } from 'lucide-react';
import { clsx } from 'clsx';
import { AppSettings } from '../types';
import changelogData from '../data/changelog.json';

interface SettingsPanelProps {
    settings: AppSettings;
    onUpdate: (settings: AppSettings) => void;
}

interface Feature {
    title: string;
    icon: string;
    color: string;
    description: string;
    link?: string;
    linkText?: string;
}

interface Release {
    version: string;
    date: string;
    badge: string;
    features: Feature[];
}

interface Suggestion {
    text: string;
    link?: string;
}

interface ChangelogData {
    currentVersion: string;
    releases: Release[];
    upcoming: string[];
    welcomeTitle?: string;
    welcomeDescription?: string;
    suggestions?: Suggestion[];
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


// Icon Lookup Map
const iconMap: Record<string, any> = {
    MousePointer2,
    Monitor,
    Moon,
    FileText
};

function ReleaseNotes() {
    const [isOpen, setIsOpen] = useState(false);
    const data = changelogData as unknown as ChangelogData;
    const { currentVersion, releases, upcoming, welcomeTitle, welcomeDescription, suggestions } = data;
    const latest = releases.find(r => r.version === currentVersion) || releases[0];

    if (!latest) return null;

    return (
        <>
            <div className="bg-surface border border-white/10 rounded-xl p-6 md:col-span-2 flex items-center justify-between cursor-pointer hover:bg-white/5 transition-colors group" onClick={() => setIsOpen(true)}>
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-yellow-500/10 rounded-lg group-hover:bg-yellow-500/20 transition-colors">
                        <FileText size={24} className="text-yellow-400" />
                    </div>
                    <div>
                        <h3 className="font-bold text-lg text-white">Release Notes</h3>
                        <p className="text-gray-400 text-sm">v{latest.version} • {latest.features.length} new features</p>
                    </div>
                </div>
                <ChevronRight className="text-gray-600 group-hover:text-white transition-colors" />
            </div>

            {isOpen && createPortal(
                <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4 animate-in fade-in duration-200">
                    <div className="bg-surface border border-white/10 rounded-2xl w-full max-w-lg shadow-2xl animate-in zoom-in-95 duration-200" onClick={e => e.stopPropagation()}>

                        {/* Header */}
                        <div className="p-6 border-b border-white/10 flex items-center justify-between">
                            <h3 className="font-bold text-xl flex items-center gap-2 text-white">
                                <FileText size={20} className="text-yellow-400" /> Release Notes
                            </h3>
                            <button
                                onClick={() => setIsOpen(false)}
                                className="p-2 hover:bg-white/10 rounded-full transition-colors text-gray-400 hover:text-white"
                            >
                                <X size={20} />
                            </button>
                        </div>

                        {/* Content */}
                        <div className="p-6 space-y-8 max-h-[70vh] overflow-y-auto">

                            {/* Welcome / Intro */}
                            <div className="space-y-2">
                                <h4 className="text-2xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-primary to-purple-400">
                                    {welcomeTitle}
                                </h4>
                                <p className="text-gray-400 leading-relaxed">
                                    {welcomeDescription}
                                </p>
                            </div>

                            {/* Latest Release */}
                            <div className="relative pl-6 border-l-2 border-primary">
                                <div className="absolute -left-[9px] top-0 w-4 h-4 rounded-full bg-primary border-4 border-surface"></div>
                                <div className="flex items-center gap-2 mb-2">
                                    <span className="font-bold text-2xl text-white">v{latest.version}</span>
                                    <span className="text-xs bg-primary/20 text-primary px-2 py-0.5 rounded-full font-medium">{latest.badge}</span>
                                    <span className="text-xs text-gray-500 ml-auto">{latest.date}</span>
                                </div>
                                <div className="space-y-4">
                                    {latest.features.map((feature, idx) => {
                                        const IconComponent = iconMap[feature.icon] || FileText;
                                        return (
                                            <div key={idx} className="bg-white/5 rounded-xl p-4">
                                                <h4 className="font-bold text-white mb-2 flex items-center gap-2">
                                                    <IconComponent size={16} className={feature.color} /> {feature.title}
                                                </h4>
                                                <p className="text-sm text-gray-400">
                                                    {feature.description}
                                                </p>
                                                {feature.link && (
                                                    <a
                                                        href={feature.link}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="text-primary hover:text-primary-light text-xs mt-3 inline-flex items-center gap-1 transition-colors"
                                                    >
                                                        {feature.linkText || "Learn more"} <ChevronRight size={12} />
                                                    </a>
                                                )}
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>

                            {/* Upcoming */}
                            <div className="relative pl-6 border-l-2 border-gray-700 opacity-60 hover:opacity-100 transition-opacity">
                                <div className="absolute -left-[9px] top-0 w-4 h-4 rounded-full bg-gray-700 border-4 border-surface"></div>
                                <div className="flex items-center gap-2 mb-2">
                                    <span className="font-bold text-lg text-gray-300">Coming Soon</span>
                                </div>
                                <ul className="text-sm text-gray-500 space-y-2 list-disc pl-4">
                                    {upcoming.map((item, idx) => (
                                        <li key={idx}>{item}</li>
                                    ))}
                                </ul>
                            </div>

                            {/* Suggestions & Issues Box */}
                            {suggestions && (
                                <div className="mt-8 bg-green-500/10 border border-green-500/20 rounded-xl p-4">
                                    <div className="flex items-center gap-2 mb-3">
                                        <div className="p-2 bg-green-500/20 rounded-lg">
                                            <FileText size={18} className="text-green-400" />
                                        </div>
                                        <span className="font-bold text-lg text-green-100">Suggestions & Issues</span>
                                    </div>
                                    <ul className="text-sm text-green-200/80 space-y-3 pl-1">
                                        {suggestions.map((item, idx) => (
                                            <li key={idx} className="flex flex-col gap-1">
                                                <span>{item.text}</span>
                                                {item.link && (
                                                    <a
                                                        href={item.link}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="text-green-400 hover:text-green-300 inline-flex items-center gap-1 transition-colors font-medium w-fit"
                                                    >
                                                        {item.text.includes("link") ? "Open Discussion" : "View Link"} <ChevronRight size={14} />
                                                    </a>
                                                )}
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            )}
                        </div>

                    </div>
                </div>,
                document.body
            )}
        </>
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
