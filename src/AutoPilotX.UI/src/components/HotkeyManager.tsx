import { useState, useEffect } from 'react';
import { Keyboard, Edit2 } from 'lucide-react';
import type { Bridge, HotkeyDefinition } from '../types';

export default function HotkeyManager() {
    const [hotkeys, setHotkeys] = useState<HotkeyDefinition[]>([]);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [listening, setListening] = useState(false);

    const bridge = window.chrome?.webview?.hostObjects?.bridge as Bridge;

    const loadHotkeys = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetHotkeys();
                setHotkeys(JSON.parse(json));
            } catch (e) {
                console.error("Failed to load hotkeys", e);
            }
        }
    };

    useEffect(() => {
        loadHotkeys();

        // Reload if macros change (renamed/deleted/added macros affect hotkey list)
        const handleListChange = () => loadHotkeys();
        window.addEventListener('MacrosListChanged', handleListChange);

        return () => {
            window.removeEventListener('MacrosListChanged', handleListChange);
        };
    }, []);

    const getKeyString = (key: number, mods: number) => {
        if (key === 0) return "None";
        // Simple mapping for display purposes - Ideally use a library or extensive map
        // This is a naive implementation for MVP
        let parts = [];
        if ((mods & 131072) !== 0) parts.push("Ctrl"); // Control
        if ((mods & 65536) !== 0) parts.push("Shift"); // Shift
        if ((mods & 262144) !== 0) parts.push("Alt"); // Alt

        let char = String.fromCharCode(key);
        if (key >= 112 && key <= 123) char = "F" + (key - 111); // F1-F12
        if (key === 27) char = "Esc";
        if (key === 13) char = "Enter";
        if (key === 32) char = "Space";
        if (key >= 48 && key <= 57) char = String.fromCharCode(key); // 0-9
        if (key >= 65 && key <= 90) char = String.fromCharCode(key); // A-Z

        parts.push(char);
        return parts.join(" + ");
    };

    const handleKeyDown = async (e: React.KeyboardEvent) => {
        if (!listening || !editingId) return;

        e.preventDefault();
        e.stopPropagation();

        // Map JS Modifiers to C# Keys Enums
        // Shift = 65536 (0x10000)
        // Control = 131072 (0x20000)
        // Alt = 262144 (0x40000)
        let mods = 0;
        if (e.shiftKey) mods |= 65536;
        if (e.ctrlKey) mods |= 131072;
        if (e.altKey) mods |= 262144;

        // Ignore modifier-only presses
        if (e.keyCode === 16 || e.keyCode === 17 || e.keyCode === 18) return;

        console.log(`Binding ${editingId} to ${e.keyCode} mod ${mods}`);

        if (bridge) {
            await bridge.SetHotkey(editingId, e.keyCode, mods);
            setListening(false);
            setEditingId(null);
            loadHotkeys();
        }
    };

    return (
        <div className="max-w-2xl animate-in fade-in" onKeyDown={listening ? handleKeyDown : undefined} tabIndex={0}>
            <div className="flex items-center gap-3 mb-8">
                <div className="p-3 bg-primary/20 rounded-xl text-primary">
                    <Keyboard size={32} />
                </div>
                <div>
                    <h2 className="text-2xl font-bold">Key Bindings</h2>
                    <p className="text-gray-400">Customize global hotkeys for your actions.</p>
                </div>
            </div>

            <div className="space-y-8">
                {/* System Hotkeys */}
                <div className="bg-surface border border-white/10 rounded-xl overflow-hidden shadow-xl">
                    <div className="p-4 bg-white/5 border-b border-white/10 font-bold text-lg text-primary">System Controls</div>
                    <table className="w-full">
                        <tbody className="divide-y divide-white/5">
                            {hotkeys.filter(h => !h.Id.startsWith('Macro:')).map(hk => (
                                <tr key={hk.Id} className="hover:bg-white/5 transition-colors group">
                                    <td className="p-4">
                                        <div className="font-medium text-white">{hk.Description}</div>
                                        <div className="text-xs text-gray-500 font-mono">{hk.Id}</div>
                                    </td>
                                    <td className="p-4">
                                        <span className="bg-white/10 text-white px-3 py-1 rounded-lg text-sm font-mono border border-white/5">
                                            {getKeyString(hk.Key, hk.Modifiers)}
                                        </span>
                                    </td>
                                    <td className="p-4 text-right">
                                        <button
                                            onClick={() => {
                                                setEditingId(hk.Id);
                                                setListening(true);
                                            }}
                                            className="text-gray-400 hover:text-primary transition-colors p-2 hover:bg-white/10 rounded-lg"
                                        >
                                            <Edit2 size={18} />
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                {/* Macro Hotkeys */}
                <div className="bg-surface border border-white/10 rounded-xl overflow-hidden shadow-xl">
                    <div className="p-4 bg-white/5 border-b border-white/10 font-bold text-lg text-green-400">Macro Bindings</div>
                    <table className="w-full">
                        <tbody className="divide-y divide-white/5">
                            {hotkeys.filter(h => h.Id.startsWith('Macro:')).length === 0 ? (
                                <tr>
                                    <td colSpan={3} className="p-8 text-center text-gray-500">
                                        No macros created yet.
                                    </td>
                                </tr>
                            ) : (
                                hotkeys.filter(h => h.Id.startsWith('Macro:')).map(hk => (
                                    <tr key={hk.Id} className="hover:bg-white/5 transition-colors group">
                                        <td className="p-4">
                                            <div className="font-medium text-white">{hk.Description}</div>
                                            <div className="text-xs text-gray-500 font-mono">{hk.Id}</div>
                                        </td>
                                        <td className="p-4">
                                            <span className="bg-white/10 text-white px-3 py-1 rounded-lg text-sm font-mono border border-white/5">
                                                {getKeyString(hk.Key, hk.Modifiers)}
                                            </span>
                                        </td>
                                        <td className="p-4 text-right">
                                            <button
                                                onClick={() => {
                                                    setEditingId(hk.Id);
                                                    setListening(true);
                                                }}
                                                className="text-gray-400 hover:text-primary transition-colors p-2 hover:bg-white/10 rounded-lg"
                                            >
                                                <Edit2 size={18} />
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Bind Modal */}
            {listening && (
                <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center animate-in fade-in">
                    <div className="bg-surface p-8 rounded-2xl border border-primary/50 shadow-2xl flex flex-col items-center gap-6 max-w-sm text-center">
                        <div className="w-20 h-20 bg-primary/20 rounded-full flex items-center justify-center animate-pulse text-primary">
                            <Keyboard size={40} />
                        </div>
                        <div>
                            <h3 className="text-2xl font-bold text-white mb-2">Press any key...</h3>
                            <p className="text-gray-400">Press the combination you want to use for this action.</p>
                        </div>
                        <button
                            onClick={() => { setListening(false); setEditingId(null); }}
                            className="text-gray-500 hover:text-white mt-4"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
