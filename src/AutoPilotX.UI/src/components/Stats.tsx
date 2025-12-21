import { useState, useEffect } from 'react';
import { BarChart3, MousePointer2, Command, Clock } from 'lucide-react';

interface StatsData {
    TotalClicks: number;
    MacrosExecuted: number;
    AutoClickerRuntimeSeconds: number;
    LastSession: string;
}

export default function Stats() {
    const [stats, setStats] = useState<StatsData | null>(null);

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    const loadStats = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetStats();
                setStats(JSON.parse(json));
            } catch (err) {
                console.error("Failed to load stats", err);
            }
        }
    };

    useEffect(() => {
        loadStats();
        // Refresh every 5s if open?
        const interval = setInterval(loadStats, 5000);
        return () => clearInterval(interval);
    }, []);

    if (!stats) return <div className="text-gray-400 p-8">Loading stats...</div>;

    return (
        <div className="p-8 max-w-6xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-4">
            <h2 className="text-3xl font-bold flex items-center gap-3">
                <BarChart3 className="text-primary" /> Statistics
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {/* Total Clicks */}
                <div className="bg-surface border border-white/5 rounded-2xl p-6 relative overflow-hidden group hover:border-primary/50 transition-colors">
                    <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
                        <MousePointer2 size={100} />
                    </div>
                    <div className="relative z-10">
                        <p className="text-gray-400 font-medium">Total Isolate Clicks</p>
                        <h3 className="text-4xl font-bold mt-2 font-mono text-blue-400">
                            {stats.TotalClicks.toLocaleString()}
                        </h3>
                    </div>
                </div>

                {/* Macros Executed */}
                <div className="bg-surface border border-white/5 rounded-2xl p-6 relative overflow-hidden group hover:border-primary/50 transition-colors">
                    <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
                        <Command size={100} />
                    </div>
                    <div className="relative z-10">
                        <p className="text-gray-400 font-medium">Macros Executed</p>
                        <h3 className="text-4xl font-bold mt-2 font-mono text-orange-400">
                            {stats.MacrosExecuted.toLocaleString()}
                        </h3>
                    </div>
                </div>

                {/* Runtime */}
                <div className="bg-surface border border-white/5 rounded-2xl p-6 relative overflow-hidden group hover:border-primary/50 transition-colors">
                    <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
                        <Clock size={100} />
                    </div>
                    <div className="relative z-10">
                        <p className="text-gray-400 font-medium">Time Saved (Runtime)</p>
                        <h3 className="text-4xl font-bold mt-2 font-mono text-green-400">
                            {(stats.AutoClickerRuntimeSeconds / 60).toFixed(1)} <span className="text-lg text-gray-500">min</span>
                        </h3>
                    </div>
                </div>
            </div>

            <div className="bg-black/20 rounded-xl p-6 border border-white/5">
                <h3 className="text-xl font-bold mb-4 text-gray-300">Activity Log</h3>
                <div className="text-gray-500 text-sm italic">
                    Detailed activity log coming soon...
                </div>
            </div>
        </div>
    );
}
