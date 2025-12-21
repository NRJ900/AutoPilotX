import { useState, useEffect } from 'react';
import { Play, Square, MousePointer, Target, Repeat, Shuffle, Crosshair } from 'lucide-react';
import { clsx } from 'clsx';
import type { Bridge } from '../types';

export default function AutoClicker() {
    const [isRunning, setIsRunning] = useState(false);

    // Settings
    const [interval, setInterval] = useState(100);
    const [randomInterval, setRandomInterval] = useState(0); // Jitter
    const [button, setButton] = useState('Left');
    const [clickType, setClickType] = useState(0); // 0=Single, 1=Double

    // Repeat
    const [repeatMode, setRepeatMode] = useState(0); // 0=Inf, 1=Count, 2=Time
    const [repeatCount, setRepeatCount] = useState(100);
    const [repeatDuration, setRepeatDuration] = useState(60); // Seconds

    // Location
    const [locationMode, setLocationMode] = useState(0); // 0=Current, 1=Fixed
    const [fixedX, setFixedX] = useState(0);
    const [fixedY, setFixedY] = useState(0);
    const [isPicking, setIsPicking] = useState(false);

    // Dynamic Hotkey Display
    const [hotkeyLabel, setHotkeyLabel] = useState('F6');

    const bridge = window.chrome?.webview?.hostObjects?.bridge as Bridge;

    const getKeyString = (key: number, mods: number) => {
        if (key === 0) return "None";
        let parts = [];
        if ((mods & 131072) !== 0) parts.push("Ctrl");
        if ((mods & 65536) !== 0) parts.push("Shift");
        if ((mods & 262144) !== 0) parts.push("Alt");

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

    useEffect(() => {
        // Sync initial state & hotkeys
        const syncState = async () => {
            if (bridge) {
                const running = await bridge.IsAutoClickerRunning();
                setIsRunning(running);

                // Fetch hotkey
                try {
                    const json = await bridge.GetHotkeys();
                    const hotkeys = JSON.parse(json);
                    const def = hotkeys.find((h: any) => h.Id === 'ToggleAutoClicker');
                    if (def) {
                        setHotkeyLabel(getKeyString(def.Key, def.Modifiers));
                    }
                } catch (e) {
                    console.error("Failed to load hotkeys", e);
                }
            }
        };
        syncState();

        const handler = (e: any) => setIsRunning(e.detail.isRunning);
        const hotkeyHandler = () => syncState();

        window.addEventListener('AutoClickerState', handler);
        window.addEventListener('HotkeysListChanged', hotkeyHandler);

        return () => {
            window.removeEventListener('AutoClickerState', handler);
            window.removeEventListener('HotkeysListChanged', hotkeyHandler);
        };
    }, []);

    const toggle = async () => {
        if (!bridge) return;

        if (isRunning) {
            await bridge.StopAutoClicker();
        } else {
            // Construct payload safely
            // Construct payload safely
            const settings = {
                IntervalMs: interval,
                RandomIntervalMs: randomInterval,
                Button: button, // "Left", "Right", "Middle"
                ClickType: clickType,
                RepeatMode: repeatMode,
                RepeatCount: repeatCount,
                RepeatDurationSeconds: repeatDuration,
                LocationMode: locationMode,
                FixedX: fixedX,
                FixedY: fixedY
            };

            await bridge.StartAutoClicker(JSON.stringify(settings));
        }
    };

    const pickLocation = async () => {
        if (!bridge) return;
        setIsPicking(true);
        try {
            const loc = await bridge.PickLocation(); // Matches C# typo
            const [x, y] = loc.split(',').map(Number);
            setFixedX(x);
            setFixedY(y);
            setLocationMode(1); // Set to Fixed
        } finally {
            setIsPicking(false);
        }
    };

    return (
        <div className="max-w-3xl animate-in fade-in space-y-6">
            <h2 className="text-2xl font-bold">Auto Clicker</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Click Interval */}
                <div className="bg-surface rounded-xl p-6 border border-white/5 shadow-lg">
                    <div className="flex items-center gap-2 mb-4 text-primary font-bold uppercase text-xs tracking-wider">
                        <Repeat size={14} /> Interval
                    </div>
                    <div className="space-y-4">
                        <div>
                            <label className="text-sm text-gray-400">Click every (ms)</label>
                            <div className="flex items-center gap-2 mt-1">
                                <input
                                    type="number"
                                    min="1"
                                    value={interval}
                                    onChange={(e) => setInterval(Number(e.target.value))}
                                    className="bg-background w-full p-2 rounded border border-white/10"
                                />
                            </div>
                        </div>
                        <div>
                            <label className="text-sm text-gray-400 flex items-center justify-between">
                                <span>Randomize (Jitter)</span>
                                <span className="text-xs bg-white/10 px-2 rounded text-gray-300">+/- {randomInterval}ms</span>
                            </label>
                            <input
                                type="range"
                                min="0"
                                max="1000"
                                value={randomInterval}
                                onChange={(e) => setRandomInterval(Number(e.target.value))}
                                className="w-full mt-2 accent-primary"
                            />
                        </div>
                    </div>
                </div>

                {/* Click Options */}
                <div className="bg-surface rounded-xl p-6 border border-white/5 shadow-lg">
                    <div className="flex items-center gap-2 mb-4 text-orange-400 font-bold uppercase text-xs tracking-wider">
                        <MousePointer size={14} /> Options
                    </div>
                    <div className="space-y-4">
                        <div>
                            <label className="text-sm text-gray-400 mb-2 block">Mouse Button</label>
                            <div className="flex bg-background rounded-lg p-1 border border-white/10">
                                {['Left', 'Right', 'Middle'].map((btn) => (
                                    <button
                                        key={btn}
                                        onClick={() => setButton(btn)}
                                        className={clsx(
                                            "flex-1 py-1.5 text-sm rounded-md transition-all",
                                            button === btn ? "bg-white/10 text-white shadow" : "text-gray-500 hover:text-gray-300"
                                        )}
                                    >
                                        {btn}
                                    </button>
                                ))}
                            </div>
                        </div>
                        <div>
                            <label className="text-sm text-gray-400 mb-2 block">Click Type</label>
                            <div className="flex gap-4">
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <input type="radio" checked={clickType === 0} onChange={() => setClickType(0)} className="accent-primary" />
                                    <span className="text-sm">Single</span>
                                </label>
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <input type="radio" checked={clickType === 1} onChange={() => setClickType(1)} className="accent-primary" />
                                    <span className="text-sm">Double</span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Repeat Mode */}
                <div className="bg-surface rounded-xl p-6 border border-white/5 shadow-lg">
                    <div className="flex items-center gap-2 mb-4 text-purple-400 font-bold uppercase text-xs tracking-wider">
                        <Shuffle size={14} /> Repeat
                    </div>
                    <div className="space-y-3">
                        <label className="flex items-center gap-3 cursor-pointer p-2 rounded hover:bg-white/5 transition-colors">
                            <input type="radio" checked={repeatMode === 0} onChange={() => setRepeatMode(0)} className="accent-primary" />
                            <div className="text-sm">
                                <div className="font-medium text-white">Repeat until stopped</div>
                                <div className="text-gray-500 text-xs text-left">Infinite loop</div>
                            </div>
                        </label>
                        <label className="flex items-center gap-3 cursor-pointer p-2 rounded hover:bg-white/5 transition-colors">
                            <input type="radio" checked={repeatMode === 1} onChange={() => setRepeatMode(1)} className="accent-primary" />
                            <div className="text-sm flex-1">
                                <div className="font-medium text-white">Repeat specific times</div>
                                {repeatMode === 1 && (
                                    <input
                                        type="number"
                                        className="mt-1 bg-background border border-white/10 rounded w-24 p-1 text-xs"
                                        value={repeatCount}
                                        onChange={e => setRepeatCount(Number(e.target.value))}
                                    />
                                )}
                            </div>
                        </label>
                        <label className="flex items-center gap-3 cursor-pointer p-2 rounded hover:bg-white/5 transition-colors">
                            <input type="radio" checked={repeatMode === 2} onChange={() => setRepeatMode(2)} className="accent-primary" />
                            <div className="text-sm flex-1">
                                <div className="font-medium text-white">Repeat for duration (s)</div>
                                {repeatMode === 2 && (
                                    <input
                                        type="number"
                                        className="mt-1 bg-background border border-white/10 rounded w-24 p-1 text-xs"
                                        value={repeatDuration}
                                        onChange={e => setRepeatDuration(Number(e.target.value))}
                                    />
                                )}
                            </div>
                        </label>
                    </div>
                </div>

                {/* Cursor Position */}
                <div className="bg-surface rounded-xl p-6 border border-white/5 shadow-lg">
                    <div className="flex items-center gap-2 mb-4 text-blue-400 font-bold uppercase text-xs tracking-wider">
                        <Target size={14} /> Position
                    </div>
                    <div className="space-y-4">
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input type="radio" checked={locationMode === 0} onChange={() => setLocationMode(0)} className="accent-primary" />
                            <span className="text-sm">Current Location</span>
                        </label>

                        <div className="flex items-start gap-2">
                            <input type="radio" checked={locationMode === 1} onChange={() => setLocationMode(1)} className="accent-primary mt-1" />
                            <div className="flex-1">
                                <span className="text-sm block mb-2">Fixed Location</span>
                                <div className="flex gap-2 mb-2">
                                    <div>
                                        <div className="text-xs text-gray-500 mb-1">X</div>
                                        <input type="number" value={fixedX} onChange={e => { setFixedX(Number(e.target.value)); setLocationMode(1); }} className="w-20 bg-background border border-white/10 rounded p-1 text-sm" />
                                    </div>
                                    <div>
                                        <div className="text-xs text-gray-500 mb-1">Y</div>
                                        <input type="number" value={fixedY} onChange={e => { setFixedY(Number(e.target.value)); setLocationMode(1); }} className="w-20 bg-background border border-white/10 rounded p-1 text-sm" />
                                    </div>
                                </div>
                                <button
                                    onClick={pickLocation}
                                    disabled={isPicking}
                                    className="text-xs bg-white/10 hover:bg-white/20 text-white px-3 py-1.5 rounded-lg flex items-center gap-2 transition-colors border border-white/5"
                                >
                                    <Crosshair size={12} />
                                    {isPicking ? "Click anywhere..." : "Pick Location"}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Main Action Button */}
            <button
                onClick={toggle}
                className={clsx(
                    "w-full py-4 rounded-xl font-bold text-lg flex items-center justify-center gap-3 transition-all transform active:scale-[0.99]",
                    isRunning
                        ? "bg-danger text-white hover:bg-danger/90 shadow-lg shadow-danger/20"
                        : "bg-primary text-white hover:bg-primary/90 shadow-lg shadow-primary/20"
                )}
            >
                {isRunning ? <Square fill="currentColor" size={20} /> : <Play fill="currentColor" size={20} />}
                {isRunning ? `Stop Clicker (${hotkeyLabel})` : `Start Clicker (${hotkeyLabel})`}
            </button>
        </div>
    );
}
