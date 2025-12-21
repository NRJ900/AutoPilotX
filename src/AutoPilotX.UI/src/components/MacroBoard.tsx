import { useState, useEffect } from 'react';
import { Play, Trash2, Plus, Square, Disc, Edit3, Save } from 'lucide-react';
import { clsx } from 'clsx';
import MacroEditor from './MacroEditor';
import type { Macro } from '../types';

export default function MacroBoard() {
    const [macros, setMacros] = useState<Macro[]>([]);
    const [isRecording, setIsRecording] = useState(false);
    const [newMacroName, setNewMacroName] = useState('');
    const [showRecordModal, setShowRecordModal] = useState(false);
    const [editingMacro, setEditingMacro] = useState<Macro | null>(null);

    // Import/Export
    const exportMacros = () => {
        const dataStr = "data:text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(macros, null, 2));
        const downloadAnchorNode = document.createElement('a');
        downloadAnchorNode.setAttribute("href", dataStr);
        downloadAnchorNode.setAttribute("download", "macros.json");
        document.body.appendChild(downloadAnchorNode); // required for firefox
        downloadAnchorNode.click();
        downloadAnchorNode.remove();
    };

    const importMacros = () => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'application/json';
        input.onchange = (e: any) => {
            const file = e.target.files[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = async (re) => {
                if (re.target?.result && bridge) {
                    try {
                        const importedButtons = JSON.parse(re.target.result as string);
                        // Validate? Arrays of macros?
                        if (Array.isArray(importedButtons)) {
                            // Upsert all? Or replace?
                            // Bridge doesn't have "Import", but supports "CreateMacro" / "UpdateMacro".
                            // Better to loop and add? Or add "ImportMacros" to bridge?
                            // Bridge.GetMacros() returns list. Bridge logic relies on memory list.
                            // Implementing "AddMacro" loop is slow if bridge calls are async one by one.
                            // But let's try looping "Create" then "Update".
                            // OR simpler: exposed "UpdateMacro" only updates if ID matches.

                            // Hacky way: Loop creates, then updates.
                            for (const m of importedButtons) {
                                // Check if exists
                                const exists = macros.find(ex => ex.Name === m.Name);
                                if (!exists) {
                                    await bridge.CreateMacro(m.Name);
                                    // Delay for creation
                                    await new Promise(r => setTimeout(r, 50));
                                }
                                // Update content
                                await bridge.UpdateMacro(JSON.stringify(m));
                            }
                            loadMacros();
                        }
                    } catch (err) {
                        alert("Invalid JSON file");
                    }
                }
            };
            reader.readAsText(file);
        };
        input.click();
    };

    // Advanced Recording State
    const [status, setStatus] = useState('');
    const [recordKeys, setRecordKeys] = useState(true);
    const [recordMouse, setRecordMouse] = useState(true);

    // Dynamic Hotkey Display
    const [hotkeyLabel, setHotkeyLabel] = useState('F9');

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    const loadMacros = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetMacros();
                const list = JSON.parse(json);
                setMacros(list);
            } catch (e) {
                console.error("Failed to parse macros", e);
            }
        }
    };

    const loadHotkeys = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetHotkeys();
                const hotkeys = JSON.parse(json);
                const def = hotkeys.find((h: any) => h.Id === 'ToggleRecording');
                if (def) {
                    // Map key code to string if needed, or rely on KeyName property? 
                    // NOTE: C# Keys enum is int. I'll need a helper or just show "Key X" if raw int. 
                    // Unlike AutoClicker.tsx where I assumed KeyName, I should verify what I get.
                    // Previous HotkeyService deserialization test in HotkeyManager.tsx used `raw integer` and mapped it locally.
                    // I should probably export `getKeyString` from HotkeyManager or duplicate it for consistency.
                    // For now, I'll assume F9 default if raw matches 120 (F9).
                    // Actually, let's just use the ID map if possible or duplicate the helper.
                    // Duplicating the helper is safest for now to be quick.
                }
                // Actually, let's just store the definition and use a mini-helper in render.
                if (def) setHotkeyLabel(getSimpleKeyName(def.Key));
            } catch (e) {
                console.error("Failed to load hotkeys", e);
            }
        }
    }

    // Mini helper for F-keys (common defaults)
    const getSimpleKeyName = (k: number) => {
        if (k >= 112 && k <= 123) return "F" + (k - 111);
        if (k === 27) return "Esc";
        return String.fromCharCode(k);
    }

    useEffect(() => {
        loadMacros();
        loadHotkeys();

        const statusHandler = (e: any) => {
            console.log("Status:", e.detail.status);
            setStatus(e.detail.status);
            if (e.detail.status === 'Recording' || e.detail.status.startsWith('Waiting')) {
                setIsRecording(true);
            } else if (e.detail.status === 'Stopped' || e.detail.status === 'Cancelled') {
                setIsRecording(false);
                setNewMacroName('');
                loadMacros();
            }
        };

        const listHandler = () => loadMacros();
        // Listen for hotkey changes too? "MacrosListChanged" covers macros, but what if I change key bind in HotkeyManager?
        // I might need to listen to generic events or refresh often.
        // For now, on mount is fine.

        window.addEventListener('MacroRecordingStatus', statusHandler);
        window.addEventListener('MacrosListChanged', listHandler);

        return () => {
            window.removeEventListener('MacroRecordingStatus', statusHandler);
            window.removeEventListener('MacrosListChanged', listHandler);
        };
    }, []);

    const prepareRecording = async () => {
        if (!newMacroName) {
            alert("Please enter a name");
            return;
        }
        if (macros.some(m => m.Name.toLowerCase() === newMacroName.toLowerCase())) {
            alert(`A macro named '${newMacroName}' already exists.`);
            return;
        }
        if (bridge) {
            try {
                await bridge.PrepareRecording(newMacroName, recordKeys, recordMouse);
                // The event listener will flip isRecording to true and set status
                setShowRecordModal(false);
            } catch (err: any) {
                console.error("PrepareRecording failed", err);
                alert("Failed to start recording: " + (err.message || err));
            }
        }
    };

    const cancelRecording = async () => {
        if (bridge) {
            await bridge.CancelRecording();
            setIsRecording(false);
        }
    };

    const playMacro = async (name: string) => {
        if (bridge) await bridge.PlayMacro(name);
    };

    const deleteMacro = async (name: string) => {
        if (bridge) {
            if (confirm(`Delete macro '${name}'?`)) {
                await bridge.DeleteMacro(name);
                loadMacros();
            }
        }
    };

    // Editor View
    if (editingMacro) {
        return (
            <MacroEditor
                macro={editingMacro}
                onSave={() => {
                    setEditingMacro(null);
                    loadMacros();
                }}
                onCancel={() => setEditingMacro(null)}
            />
        );
    }

    return (
        <div className="max-w-4xl animate-in fade-in">
            <div className="flex items-center justify-between mb-8">
                <h2 className="text-2xl font-bold">Macro Board</h2>
                {!isRecording && (
                    <div className="flex gap-2">
                        <button onClick={exportMacros} className="p-2 text-gray-400 hover:text-white hover:bg-white/10 rounded-lg transition-colors" title="Export Macros">
                            <Disc size={20} />
                        </button>
                        <button onClick={importMacros} className="p-2 text-gray-400 hover:text-white hover:bg-white/10 rounded-lg transition-colors" title="Import Macros">
                            <Save size={20} className="rotate-180" />
                        </button>
                        <button
                            onClick={() => {
                                cancelRecording(); // Reset state just in case
                                setShowRecordModal(true);
                            }}
                            className="bg-primary hover:bg-primary/90 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-all hover:scale-105 shadow-lg shadow-primary/20"
                        >
                            <Plus size={18} />
                            Record New
                        </button>
                    </div>
                )}
            </div>

            {/* Recording Overlay */}
            {isRecording && (
                <div className="fixed inset-0 bg-black/80 z-50 flex flex-col items-center justify-center animate-in fade-in">
                    <div className="bg-surface p-8 rounded-2xl border border-primary/20 flex flex-col items-center gap-6 shadow-2xl w-full max-w-md">
                        <div className={clsx("w-16 h-16 rounded-full flex items-center justify-center animate-pulse",
                            status === 'Recording' ? "bg-danger/20 text-danger" : "bg-primary/20 text-primary")}>
                            {status === 'Recording' ? <Disc size={32} /> : <Square size={32} />}
                        </div>
                        <div className="text-center w-full">
                            <h3 className="text-2xl font-bold text-white mb-2">{status}</h3>
                            <p className="text-gray-400">Press <span className="text-white font-bold bg-white/10 px-2 rounded">{hotkeyLabel}</span> to Start/Stop</p>

                            {/* Manual Start Button */}
                            {status.startsWith("Waiting") && (
                                <button
                                    onClick={() => bridge?.ToggleRecording()}
                                    className="mt-4 bg-primary hover:bg-primary/90 text-white px-6 py-2 rounded-lg font-bold shadow-lg shadow-primary/20 transition-all hover:scale-105 w-full"
                                >
                                    Start Now
                                </button>
                            )}

                            {/* Manual Stop Button */}
                            {status === "Recording..." && (
                                <button
                                    onClick={() => bridge?.ToggleRecording()}
                                    className="mt-4 bg-danger hover:bg-danger/90 text-white px-6 py-2 rounded-lg font-bold shadow-lg shadow-danger/20 transition-all hover:scale-105 flex items-center gap-2 justify-center w-full"
                                >
                                    <Square size={20} fill="currentColor" />
                                    Stop Recording
                                </button>
                            )}
                        </div>
                        <button
                            onClick={cancelRecording}
                            className="text-gray-400 hover:text-white mt-4 border border-white/10 px-4 py-2 rounded-lg hover:bg-white/5 transition-colors w-full"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {/* Record Modal */}
            {showRecordModal && (
                <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center animate-in zoom-in-95">
                    <div className="bg-surface p-8 rounded-2xl border border-white/10 w-full max-w-md shadow-2xl space-y-6">
                        <h3 className="text-2xl font-bold text-center">New Macro</h3>
                        <input
                            autoFocus
                            type="text"
                            placeholder="Macro Name"
                            className="w-full bg-background border border-white/10 rounded-lg px-4 py-2 focus:border-primary focus:outline-none"
                            value={newMacroName}
                            onChange={e => setNewMacroName(e.target.value)}
                            onKeyDown={e => e.key === 'Enter' && prepareRecording()}
                        />

                        <div className="flex gap-4 select-none">
                            <label className="flex items-center gap-2 cursor-pointer group">
                                <input type="checkbox" checked={recordKeys} onChange={e => setRecordKeys(e.target.checked)} className="accent-primary" />
                                <span className="text-sm text-gray-400 group-hover:text-white transition-colors">Record Keys</span>
                            </label>
                            <label className="flex items-center gap-2 cursor-pointer group">
                                <input type="checkbox" checked={recordMouse} onChange={e => setRecordMouse(e.target.checked)} className="accent-primary" />
                                <span className="text-sm text-gray-400 group-hover:text-white transition-colors">Record Mouse</span>
                            </label>
                        </div>

                        <div className="flex justify-end gap-2 pt-2">
                            <button onClick={() => setShowRecordModal(false)} className="px-4 py-2 text-gray-400 hover:text-white">Cancel</button>
                            <button
                                onClick={async () => {
                                    if (!newMacroName) return;
                                    if (macros.some(m => m.Name.toLowerCase() === newMacroName.toLowerCase())) {
                                        alert(`A macro named '${newMacroName}' already exists.`);
                                        return;
                                    }
                                    if (bridge) {
                                        await bridge.CreateMacro(newMacroName);
                                        // Wait for backend to save
                                        await new Promise(r => setTimeout(r, 100));
                                        await loadMacros();

                                        // Find and open
                                        setTimeout(async () => {
                                            const json = await bridge.GetMacros();
                                            const list = JSON.parse(json);
                                            const m = list.find((x: any) => x.Name === newMacroName);
                                            if (m) setEditingMacro(m);
                                        }, 100);

                                        setShowRecordModal(false);
                                    }
                                }}
                                className="bg-surface hover:bg-primary/10 text-white border border-white/10 px-4 py-2 rounded-lg font-medium transition-colors"
                            >
                                Create Empty
                            </button>
                            <button
                                onClick={prepareRecording}
                                className="bg-primary hover:bg-primary/90 text-white px-4 py-2 rounded-lg font-medium transition-colors"
                            >
                                Record
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Macro Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {macros.length === 0 ? (
                    <div className="col-span-full text-center py-20 text-gray-500 border border-dashed border-white/10 rounded-xl bg-white/5">
                        <Square size={48} className="mx-auto mb-4 opacity-50" />
                        <p className="text-lg font-medium">No macros recorded yet</p>
                        <p className="text-sm opacity-50">Click "Record New" to get started</p>
                    </div>
                ) : (
                    macros.map((macro, i) => (
                        <div key={i} className="bg-surface border border-white/5 p-4 rounded-xl flex items-center justify-between hover:border-white/10 transition-all hover:bg-white/5 group">
                            <div>
                                <h3 className="font-bold text-lg text-white">{macro.Name}</h3>
                                <p className="text-sm text-gray-400 flex items-center gap-2">
                                    <Square size={12} />
                                    {macro.Actions?.length || 0} actions
                                </p>
                            </div>
                            <div className="flex items-center gap-2">
                                <button
                                    onClick={() => setEditingMacro(macro)}
                                    className="p-2 hover:bg-white/10 rounded-lg text-blue-400 transition-colors"
                                    title="Edit"
                                >
                                    <Edit3 size={18} />
                                </button>
                                <button
                                    onClick={() => playMacro(macro.Name)}
                                    className="p-2 hover:bg-white/10 rounded-lg text-success transition-colors"
                                    title="Play"
                                >
                                    <Play size={20} fill="currentColor" />
                                </button>
                                <button
                                    onClick={() => deleteMacro(macro.Name)}
                                    className="p-2 hover:bg-white/10 rounded-lg text-gray-500 hover:text-danger transition-colors opacity-0 group-hover:opacity-100"
                                    title="Delete"
                                >
                                    <Trash2 size={18} />
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
}
