import { useState, useEffect } from 'react';
import { Pause, Maximize2, Move } from 'lucide-react';

interface MiniOverlayProps {
    onExpand: () => void;
}

export default function MiniOverlay({ onExpand }: MiniOverlayProps) {
    const [isRunning, setIsRunning] = useState(false);
    const [status, setStatus] = useState("Ready");

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    useEffect(() => {
        // Poll status
        const checkStatus = async () => {
            if (bridge) {
                // Check running state for button logic (simple heuristic)
                // Getting detailed text
                const statusText = await bridge.GetDetailedStatus();

                // If text contains "Playing" or "Clicking", we consider it active/busy
                const isBusy = statusText !== "Ready";

                setIsRunning(isBusy);
                setStatus(statusText);
            }
        };

        const interval = setInterval(checkStatus, 500);
        return () => clearInterval(interval);
    }, []);

    const toggle = async () => {
        if (!bridge) return;

        // Stop whatever is running
        if (status.startsWith("Auto Clicking")) {
            await bridge.StopAutoClicker();
        } else if (status.startsWith("Playing")) {
            await bridge.StopMacro();
        }

        // Update immediately for better UX
        setIsRunning(false);
        setStatus("Stopped");
    };

    const [lastClickTime, setLastClickTime] = useState(0);

    const handleDrag = () => {
        const now = Date.now();
        if (now - lastClickTime < 300) {
            // Double click detected -> Expand
            onExpand();
        } else {
            // Single click -> Drag
            setLastClickTime(now);
            if (bridge) bridge.DragWindow();
        }
    };

    return (
        <div className="h-screen w-full bg-surface border border-white/20 flex items-center px-4 gap-4 overflow-hidden">
            {/* Drag Indicator */}
            <div
                className="cursor-move text-gray-500 hover:text-white"
                title="Drag Window"
                onMouseDown={handleDrag}
            >
                <Move size={16} />
            </div>

            {/* Status */}
            <div className="flex-1" onMouseDown={handleDrag}>
                <div className="text-xs text-uppercase font-bold text-gray-400 select-none">Status</div>
                <div className={`font-bold ${isRunning ? 'text-green-400' : 'text-blue-400'}`}>
                    {status}
                </div>
            </div>

            {/* Control (Stop only) */}
            {isRunning && (
                <button
                    onClick={toggle}
                    className="p-2 rounded-full bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white transition-colors"
                >
                    <Pause size={18} fill="currentColor" />
                </button>
            )}

            {/* Expand */}
            <button
                onClick={onExpand}
                className="p-2 rounded-lg hover:bg-white/10 text-gray-400 hover:text-white transition-colors"
                title="Expand View"
            >
                <Maximize2 size={18} />
            </button>
        </div>
    );
}
