import { useState, useEffect } from 'react';
import { Save, Trash2, Clock, MousePointer2, Keyboard, Target, GripVertical } from 'lucide-react';
import { clsx } from 'clsx';
import { Reorder } from 'framer-motion';
import type { Macro, MacroAction } from '../types';

// Wrapper for DnD stability
interface EditableAction extends MacroAction {
    _id: string;
}

interface MacroEditorProps {
    macro: Macro;
    onSave: () => void;
    onCancel: () => void;
}

export default function MacroEditor({ macro, onSave, onCancel }: MacroEditorProps) {
    const [actions, setActions] = useState<EditableAction[]>([]);
    const [capturingKey, setCapturingKey] = useState(false);

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    useEffect(() => {
        // Hydrate with IDs for Framer Motion
        const hydrated = (macro.Actions || []).map(a => ({
            ...a,
            _id: crypto.randomUUID()
        }));
        setActions(hydrated);

        setRepeatMode(macro.RepeatMode || 0);
        setRepeatCount(macro.RepeatCount || 0);
        setSpeed(macro.SpeedMultiplier || 1.0);
        setIsRelative(macro.IsRelative || false);
    }, [macro]);

    // Settings State
    const [repeatMode, setRepeatMode] = useState(0);
    const [repeatCount, setRepeatCount] = useState(0);
    const [speed, setSpeed] = useState(1.0);
    const [isRelative, setIsRelative] = useState(false);

    // Cleanup listener
    useEffect(() => {
        if (!capturingKey) return;

        let handleKeyDown: (e: KeyboardEvent) => void;

        // precise delay to prevention accidental triggers (like the Enter key used to open this)
        const timer = setTimeout(() => {
            handleKeyDown = (e: KeyboardEvent) => {
                e.preventDefault();
                e.stopPropagation();
                // Add Key Press Sequence (Down -> Delay -> Up)
                addKeySequence(e.key);
                setCapturingKey(false);
            };
            window.addEventListener('keydown', handleKeyDown);
        }, 150);

        return () => {
            clearTimeout(timer);
            if (handleKeyDown) window.removeEventListener('keydown', handleKeyDown);
        };
    }, [capturingKey, actions]);

    const saveChanges = async () => {
        if (bridge) {
            // Strip IDs before saving
            const cleanActions = actions.map(({ _id, ...rest }) => rest);

            const updatedMacro = {
                ...macro,
                Actions: cleanActions,
                RepeatMode: repeatMode,
                RepeatCount: repeatCount,
                SpeedMultiplier: speed,
                IsRelative: isRelative
            };
            await bridge.UpdateMacro(JSON.stringify(updatedMacro));
            onSave();
        }
    };

    const updateAction = (index: number, updates: Partial<EditableAction>) => {
        const newActions = [...actions];
        newActions[index] = { ...newActions[index], ...updates };
        setActions(newActions);
    };

    const deleteAction = (index: number) => {
        setActions(actions.filter((_, i) => i !== index));
    };

    const addDelay = () => {
        setActions([...actions, { _id: crypto.randomUUID(), Type: 4, X: 0, Y: 0, Button: 0, Key: '', DelayMs: 1000 }]);
    };

    const addMouse = () => {
        setActions([...actions, { _id: crypto.randomUUID(), Type: 1, X: 0, Y: 0, Button: 0, Key: '', DelayMs: 50 }]); // Default Left Click at 0,0
    };

    const mapKeyName = (key: string): string => {
        // Map JS key names to C# Keys enum names
        if (key === 'ArrowUp') return 'Up';
        if (key === 'ArrowDown') return 'Down';
        if (key === 'ArrowLeft') return 'Left';
        if (key === 'ArrowRight') return 'Right';
        if (key === ' ') return 'Space';
        if (key === 'Escape') return 'Escape'; // already matches
        if (key === 'Enter') return 'Enter';   // already matches
        if (key === 'Control') return 'ControlKey';
        if (key === 'Shift') return 'ShiftKey';
        if (key === 'Alt') return 'Menu'; // Alt is Menu in WinForms Keys

        // Single letters are fine, functional keys usually fine (F1..F12)
        return key.length === 1 ? key.toUpperCase() : key;
    };

    const addKeySequence = (rawKey: string) => {
        const key = mapKeyName(rawKey);
        // Appends Down -> Delay -> Up
        const newActions = [...actions];
        newActions.push({ _id: crypto.randomUUID(), Type: 2, X: 0, Y: 0, Button: 0, Key: key, DelayMs: 50 }); // Down (holds for 50ms)
        newActions.push({ _id: crypto.randomUUID(), Type: 3, X: 0, Y: 0, Button: 0, Key: key, DelayMs: 50 }); // Up (delay after is 50ms)
        setActions(newActions);
    };

    const getIcon = (type: number) => {
        switch (type) {
            case 0: return <MousePointer2 size={16} className="text-blue-400" />;
            case 1: return <MousePointer2 size={16} className="text-green-400" />;
            case 2: return <Keyboard size={16} className="text-orange-400" />;
            case 3: return <Keyboard size={16} className="text-orange-400 opacity-50" />;
            case 4: return <Clock size={16} className="text-gray-400" />;
            default: return <Clock size={16} />;
        }
    };

    return (
        <div className="fixed inset-0 bg-black/90 z-50 flex items-center justify-center p-8 animate-in fade-in slide-in-from-bottom-4">
            <div className="bg-surface w-full max-w-5xl h-[80vh] rounded-2xl border border-white/10 flex flex-col shadow-2xl relative">

                {/* Header */}
                <div className="p-6 border-b border-white/10 flex items-center justify-between bg-white/5">
                    <div>
                        <h2 className="text-2xl font-bold flex items-center gap-3">
                            Edit Macro: <span className="text-primary">{macro.Name}</span>
                        </h2>
                        <p className="text-gray-400 text-sm mt-1">{actions.length} actions in sequence</p>
                    </div>
                    <div className="flex gap-3">
                        <button onClick={onCancel} className="px-4 py-2 rounded-lg hover:bg-white/10 text-gray-400 hover:text-white transition-colors">
                            Cancel
                        </button>
                        <button onClick={saveChanges} className="px-6 py-2 rounded-lg bg-primary hover:bg-primary/90 text-white font-bold transition-colors flex items-center gap-2 shadow-lg shadow-primary/20">
                            <Save size={20} /> Save Changes
                        </button>
                    </div>
                </div>

                {/* Settings Toolbar */}
                <div className="p-4 bg-black/40 border-b border-white/5 flex flex-wrap items-center gap-6 justify-center text-sm">
                    {/* Speed */}
                    <div className="flex items-center gap-2">
                        <span className="text-gray-400">Speed:</span>
                        <input
                            type="range" min="0.1" max="5.0" step="0.1"
                            value={speed} onChange={e => setSpeed(Number(e.target.value))}
                            className="w-24 accent-primary"
                        />
                        <span className="w-10 font-mono text-right">{speed.toFixed(1)}x</span>
                    </div>

                    {/* Loop */}
                    <div className="flex items-center gap-2 border-l border-white/10 pl-6">
                        <span className="text-gray-400">Loop:</span>
                        <select
                            value={repeatMode} onChange={e => setRepeatMode(Number(e.target.value))}
                            className="bg-black/20 border border-white/10 rounded px-2 py-1 focus:outline-none"
                        >
                            <option value={0}>Infinite</option>
                            <option value={1}>Count</option>
                            <option value={2}>Once</option>
                        </select>
                        <input
                            type="number"
                            disabled={repeatMode !== 1}
                            value={repeatCount} onChange={e => setRepeatCount(Number(e.target.value))}
                            placeholder="Count"
                            className={clsx("w-16 bg-black/20 border border-white/10 rounded px-2 py-1 focus:outline-none transition-opacity", repeatMode !== 1 && "opacity-50")}
                        />
                    </div>

                    {/* Relative */}
                    <div className="flex items-center gap-2 border-l border-white/10 pl-6 cursor-pointer">
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input type="checkbox" checked={isRelative} onChange={e => setIsRelative(e.target.checked)} className="accent-primary" />
                            <span className={clsx(isRelative ? "text-white" : "text-gray-400")}>Relative Mode</span>
                        </label>
                    </div>
                </div>

                {/* Adjustment Toolbar */}
                <div className="p-4 bg-black/20 border-b border-white/5 flex gap-4 justify-center">
                    <button onClick={() => setCapturingKey(true)} className="flex items-center gap-2 px-4 py-2 bg-surface border border-white/10 rounded-lg hover:bg-white/5 transition-colors text-orange-400">
                        <Keyboard size={18} /> Add Key Press
                    </button>
                    <button onClick={addMouse} className="flex items-center gap-2 px-4 py-2 bg-surface border border-white/10 rounded-lg hover:bg-white/5 transition-colors text-green-400">
                        <MousePointer2 size={18} /> Add Mouse Click
                    </button>
                    <button onClick={addDelay} className="flex items-center gap-2 px-4 py-2 bg-surface border border-white/10 rounded-lg hover:bg-white/5 transition-colors text-gray-400">
                        <Clock size={18} /> Add Delay
                    </button>
                </div>

                {/* Action List */}
                <div className="flex-1 overflow-y-auto p-6 space-y-2">
                    <Reorder.Group axis="y" values={actions} onReorder={setActions} className="space-y-2">
                        {actions.map((action, i) => (
                            <Reorder.Item key={action._id} value={action} className="group relative pl-8 pb-2">
                                {/* Node Icon */}
                                <div className="absolute left-0 top-0 w-10 h-10 rounded-full bg-surface border border-white/10 flex items-center justify-center z-10 group-hover:border-primary/50 transition-colors shadow-lg">
                                    {getIcon(action.Type)}
                                </div>

                                {/* Card Content */}
                                <div className="ml-4 bg-background/50 border border-white/5 rounded-xl p-3 hover:border-white/10 transition-colors flex items-center justify-between group-hover:bg-white/5 cursor-default">
                                    {/* Drag Handle */}
                                    <div className="mr-3 text-gray-600 hover:text-white cursor-grab active:cursor-grabbing">
                                        <GripVertical size={16} />
                                    </div>

                                    <span className="text-sm font-mono text-gray-500 w-6">#{i + 1}</span>

                                    {/* Dynamic Inputs */}
                                    <div className="flex-1 px-4 flex gap-4 items-center flex-wrap">
                                        {(action.Type === 2 || action.Type === 3) && (
                                            <div className="flex items-center gap-2">
                                                <span className={clsx("font-bold w-20", action.Type === 2 ? "text-orange-400" : "text-orange-400/50")}>
                                                    {action.Type === 2 ? "Key Down" : "Key Up"}
                                                </span>
                                                <input
                                                    className="bg-black/20 border border-white/10 rounded px-2 py-1 w-24 text-center text-sm font-mono text-white focus:border-primary focus:outline-none"
                                                    value={action.Key}
                                                    onChange={e => updateAction(i, { Key: e.target.value })}
                                                    onPointerDown={(e) => e.stopPropagation()} // Prevent drag start on input focus
                                                    onKeyDown={(e) => e.stopPropagation()}
                                                />
                                            </div>
                                        )}

                                        {(action.Type === 0 || action.Type === 1) && (
                                            <div className="flex items-center gap-2">
                                                <span className="font-bold text-blue-400 w-20">{action.Type === 0 ? "Move" : "Click"}</span>
                                                <span className="text-gray-500">X:</span>
                                                <input
                                                    className="bg-black/20 border border-white/10 rounded px-2 py-1 w-16 text-right text-sm font-mono text-white focus:border-primary focus:outline-none"
                                                    type="number" value={action.X} onChange={e => updateAction(i, { X: parseInt(e.target.value) || 0 })}
                                                    onPointerDown={(e) => e.stopPropagation()}
                                                />
                                                <span className="text-gray-500">Y:</span>
                                                <input
                                                    className="bg-black/20 border border-white/10 rounded px-2 py-1 w-16 text-right text-sm font-mono text-white focus:border-primary focus:outline-none"
                                                    type="number" value={action.Y} onChange={e => updateAction(i, { Y: parseInt(e.target.value) || 0 })}
                                                    onPointerDown={(e) => e.stopPropagation()}
                                                />

                                                {/* Pick Location Button */}
                                                <button
                                                    title="Pick Location from Screen"
                                                    onClick={async () => {
                                                        if (bridge) {
                                                            try {
                                                                // Calls PickLocation
                                                                const loc = await bridge.PickLocation();
                                                                const parts = loc.split(',');
                                                                if (parts.length === 2) {
                                                                    updateAction(i, {
                                                                        X: parseInt(parts[0]),
                                                                        Y: parseInt(parts[1])
                                                                    });
                                                                }
                                                            } catch (err) {
                                                                console.error("Pick failed", err);
                                                            }
                                                        }
                                                    }}
                                                    className="p-1.5 bg-black/20 hover:bg-primary/20 hover:text-primary rounded border border-white/10 transition-colors"
                                                    onPointerDown={(e) => e.stopPropagation()}
                                                >
                                                    <Target size={14} />
                                                </button>

                                                {action.Type === 1 && (
                                                    <select
                                                        className="bg-black/20 border border-white/10 rounded px-2 py-1 text-sm text-white focus:border-primary focus:outline-none"
                                                        value={action.Button}
                                                        onChange={e => updateAction(i, { Button: parseInt(e.target.value) })}
                                                        onPointerDown={(e) => e.stopPropagation()}
                                                    >
                                                        <option value={0}>Left</option>
                                                        <option value={1}>Right</option>
                                                        <option value={2}>Middle</option>
                                                    </select>
                                                )}
                                            </div>
                                        )}

                                        {action.Type === 4 && (
                                            <div className="flex items-center gap-2">
                                                <span className="font-bold text-gray-400 w-20">Wait</span>
                                            </div>
                                        )}
                                    </div>

                                    {/* Common Delay & Delete */}
                                    <div className="flex items-center gap-4">
                                        <div className="flex items-center gap-2 bg-black/20 px-3 py-1 rounded-lg border border-white/5 focus-within:border-primary/50 transition-colors">
                                            <Clock size={14} className="text-gray-500" />
                                            <input
                                                type="number"
                                                value={action.DelayMs}
                                                onChange={(e) => updateAction(i, { DelayMs: parseInt(e.target.value) || 0 })}
                                                className="w-16 bg-transparent text-right text-sm focus:outline-none font-mono"
                                                onPointerDown={(e) => e.stopPropagation()}
                                            />
                                            <span className="text-xs text-gray-500">ms</span>
                                        </div>
                                        <button onClick={() => deleteAction(i)} className="p-2 hover:bg-red-500/20 text-gray-500 hover:text-red-400 rounded-lg transition-colors">
                                            <Trash2 size={18} />
                                        </button>
                                    </div>
                                </div>
                            </Reorder.Item>
                        ))}
                    </Reorder.Group>
                </div>

                {/* Capture Overlay */}
                {capturingKey && (
                    <div className="absolute inset-0 bg-black/80 z-20 flex flex-col items-center justify-center rounded-2xl animate-in fade-in">
                        <Keyboard size={64} className="text-primary mb-4 animate-bounce" />
                        <h3 className="text-2xl font-bold mb-2">Press Any Key</h3>
                        <p className="text-gray-400">It will be added to the macro sequence.</p>
                        <button onClick={() => setCapturingKey(false)} className="mt-8 px-6 py-2 border border-white/10 rounded-lg hover:bg-white/10">Cancel</button>
                    </div>
                )}
            </div>
        </div>
    );
}
