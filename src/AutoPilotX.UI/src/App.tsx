import { useState, useEffect } from 'react';
import { MousePointer2, Keyboard, Command, Settings, BarChart3, Folder, ChevronLeft, ChevronRight } from 'lucide-react';
import { clsx } from 'clsx';
import { motion, AnimatePresence } from 'framer-motion';
import AutoClicker from './components/AutoClicker';
import MacroBoard from './components/MacroBoard';
import HotkeyManager from './components/HotkeyManager';
import Stats from './components/Stats';
import Profiles from './components/Profiles';
import SettingsPanel from './components/Settings';
import { AppSettings } from './types';

function App() {
    const [activeTab, setActiveTab] = useState('autoclicker');
    const [isCollapsed, setIsCollapsed] = useState(false);
    const [settings, setSettings] = useState<AppSettings>({
        AlwaysOnTop: true,
        MinimizeToTray: false,
        Theme: 'Dark',
        SoundEffects: true
    });

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    useEffect(() => {
        loadSettings();
    }, []);

    const loadSettings = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetSettings();
                const loaded = JSON.parse(json);
                setSettings({ ...settings, ...loaded });
            } catch (err) {
                console.error("Failed to load settings", err);
            }
        }
    };

    const handleSettingChange = async (newSettings: AppSettings) => {
        setSettings(newSettings);
        if (bridge) {
            await bridge.SaveSettings(JSON.stringify(newSettings));
        }
    };

    const navItems = [
        { id: 'autoclicker', icon: MousePointer2, label: 'Auto Clicker' },
        { id: 'macros', icon: Command, label: 'Macros' },
        { id: 'hotkeys', icon: Keyboard, label: 'Hotkeys' },
        { id: 'stats', icon: BarChart3, label: 'Stats' },
        { id: 'profiles', icon: Folder, label: 'Profiles' },
        { id: 'settings', icon: Settings, label: 'Settings' },
    ];

    return (
        <div className="flex h-screen bg-background text-white select-none overflow-hidden">

            {/* Dynamic Sidebar */}
            <motion.div
                className="bg-surface/90 border-r border-white/5 flex flex-col relative z-20"
                initial={false}
                animate={{ width: isCollapsed ? 80 : 256 }}
                transition={{ duration: 0.3, ease: "easeInOut" }}
            >
                <div className="p-4 flex items-center justify-between">
                    <AnimatePresence mode='wait'>
                        {!isCollapsed && (
                            <motion.h1
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                className="text-xl font-bold bg-gradient-to-r from-primary to-blue-400 bg-clip-text text-transparent px-2 whitespace-nowrap"
                            >
                                AutoPilotX
                            </motion.h1>
                        )}
                    </AnimatePresence>

                    <button
                        onClick={() => setIsCollapsed(!isCollapsed)}
                        className="p-1.5 rounded-full hover:bg-white/10 text-gray-400 hover:text-white transition-colors absolute right-[-12px] top-6 bg-surface border border-white/10 shadow-lg z-50"
                    >
                        {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
                    </button>
                </div>

                <div className="flex-1 px-3 py-2 space-y-2 overflow-y-auto overflow-x-hidden">
                    {navItems.map((item) => (
                        <button
                            key={item.id}
                            onClick={() => setActiveTab(item.id)}
                            className={clsx(
                                "flex items-center gap-3 px-3 py-3 rounded-lg transition-all text-sm font-medium w-full relative group",
                                activeTab === item.id
                                    ? "bg-primary text-white shadow-lg shadow-primary/20"
                                    : "text-gray-400 hover:bg-white/5 hover:text-white"
                            )}
                            title={isCollapsed ? item.label : undefined}
                        >
                            <item.icon size={20} className="shrink-0" />

                            <AnimatePresence>
                                {!isCollapsed && (
                                    <motion.span
                                        initial={{ opacity: 0, x: -10 }}
                                        animate={{ opacity: 1, x: 0 }}
                                        exit={{ opacity: 0, x: -10 }}
                                        transition={{ duration: 0.2 }}
                                        className="whitespace-nowrap"
                                    >
                                        {item.label}
                                    </motion.span>
                                )}
                            </AnimatePresence>

                            {/* Active Indicator Strip */}
                            {activeTab === item.id && (
                                <motion.div
                                    layoutId="activeStrip"
                                    className="absolute left-0 top-2 bottom-2 w-1 bg-white rounded-r-full"
                                />
                            )}
                        </button>
                    ))}
                </div>
            </motion.div>

            {/* Main Content */}
            <div className="flex-1 p-8 overflow-y-auto bg-background/50 backdrop-blur-sm relative z-10">
                {activeTab === 'autoclicker' && <AutoClicker />}
                {activeTab === 'macros' && <MacroBoard />}
                {activeTab === 'hotkeys' && <HotkeyManager />}
                {activeTab === 'stats' && <Stats />}
                {activeTab === 'profiles' && <Profiles />}
                {activeTab === 'settings' && <SettingsPanel settings={settings} onUpdate={handleSettingChange} />}
            </div>
        </div>
    );
}

export default App;
