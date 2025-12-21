import { useState } from 'react';
import { MousePointer2, Keyboard, Command, Settings, BarChart3, Folder } from 'lucide-react';
import { clsx } from 'clsx';
import AutoClicker from './components/AutoClicker';
import MacroBoard from './components/MacroBoard';
import HotkeyManager from './components/HotkeyManager';
import Stats from './components/Stats';
import Profiles from './components/Profiles';
import SettingsPanel from './components/Settings';

function App() {
    const [activeTab, setActiveTab] = useState('autoclicker');

    const navItems = [
        { id: 'autoclicker', icon: MousePointer2, label: 'Auto Clicker' },
        { id: 'macros', icon: Command, label: 'Macros' },
        { id: 'hotkeys', icon: Keyboard, label: 'Hotkeys' },
        { id: 'stats', icon: BarChart3, label: 'Stats' },
        { id: 'profiles', icon: Folder, label: 'Profiles' },
        { id: 'settings', icon: Settings, label: 'Settings' },
    ];

    return (
        <div className="flex h-screen bg-background text-white select-none">
            {/* Sidebar */}
            <div className="w-64 bg-surface p-4 flex flex-col gap-2 border-r border-white/5">
                <h1 className="text-xl font-bold mb-6 px-4 bg-gradient-to-r from-primary to-blue-400 bg-clip-text text-transparent">
                    AutoPilotX
                </h1>

                {navItems.map((item) => (
                    <button
                        key={item.id}
                        onClick={() => setActiveTab(item.id)}
                        className={clsx(
                            "flex items-center gap-3 px-4 py-3 rounded-lg transition-all text-sm font-medium",
                            activeTab === item.id
                                ? "bg-primary text-white shadow-lg shadow-primary/20"
                                : "text-gray-400 hover:bg-white/5 hover:text-white"
                        )}
                    >
                        <item.icon size={20} />
                        {item.label}
                    </button>
                ))}
            </div>

            {/* Main Content */}
            <div className="flex-1 p-8 overflow-y-auto">
                {activeTab === 'autoclicker' && <AutoClicker />}
                {activeTab === 'macros' && <MacroBoard />}
                {activeTab === 'hotkeys' && <HotkeyManager />}
                {activeTab === 'stats' && <Stats />}
                {activeTab === 'profiles' && <Profiles />}
                {activeTab === 'settings' && <SettingsPanel />}
            </div>
        </div>
    );
}

export default App;
