import { useState, useEffect } from 'react';
import { Folder, Plus, Trash2, Check, User } from 'lucide-react';
import { clsx } from 'clsx';

export default function Profiles() {
    const [profiles, setProfiles] = useState<string[]>([]);
    const [currentProfile, setCurrentProfile] = useState<string>('');
    const [isCreating, setIsCreating] = useState(false);
    const [newProfileName, setNewProfileName] = useState('');

    const bridge = window.chrome?.webview?.hostObjects?.bridge;

    const loadProfiles = async () => {
        if (bridge) {
            try {
                const json = await bridge.GetProfiles();
                const list = JSON.parse(json);
                setProfiles(list);

                const current = await bridge.GetCurrentProfile();
                setCurrentProfile(current);
            } catch (err) {
                console.error("Failed to load profiles", err);
            }
        }
    };

    useEffect(() => {
        loadProfiles();
    }, []);

    const handleCreate = async () => {
        if (!newProfileName.trim() || !bridge) return;

        await bridge.CreateProfile(newProfileName.trim());
        setNewProfileName('');
        setIsCreating(false);
        loadProfiles();
    };

    const handleSwitch = async (name: string) => {
        if (!bridge) return;
        await bridge.SwitchProfile(name);

        // Give backend a moment to swap files and reload
        setTimeout(async () => {
            await loadProfiles();
            // Reload page to refresh all data? 
            // Ideally we shouldn't need to refresh page, but other components (Macros/Hotkeys) 
            // need to know data changed.
            // Bridge emits 'MacrosListChanged', so MacroBoard should auto-update.
            // We just need to update local profile state.
        }, 200);
    };

    const handleDelete = async (name: string, e: React.MouseEvent) => {
        e.stopPropagation();
        if (!bridge || !confirm(`Delete profile '${name}'?`)) return;

        await bridge.DeleteProfile(name);
        loadProfiles();
    };

    return (
        <div className="p-8 max-w-4xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-4">
            <div className="flex items-center justify-between">
                <h2 className="text-3xl font-bold flex items-center gap-3">
                    <Folder className="text-primary" /> Profiles
                </h2>
                <button
                    onClick={() => setIsCreating(true)}
                    className="px-4 py-2 bg-primary hover:bg-primary/90 text-white rounded-lg flex items-center gap-2 transition-colors shadow-lg shadow-primary/20"
                >
                    <Plus size={20} /> New Profile
                </button>
            </div>

            {/* Create Input */}
            {isCreating && (
                <div className="bg-surface border border-white/10 rounded-xl p-4 flex gap-4 animate-in slide-in-from-top-2">
                    <input
                        autoFocus
                        value={newProfileName}
                        onChange={e => setNewProfileName(e.target.value)}
                        placeholder="Profile Name (e.g. Minecraft, Work)"
                        className="flex-1 bg-black/20 border border-white/10 rounded-lg px-4 py-2 text-white focus:border-primary focus:outline-none"
                        onKeyDown={e => e.key === 'Enter' && handleCreate()}
                    />
                    <button onClick={handleCreate} className="px-6 py-2 bg-green-500/20 text-green-400 hover:bg-green-500/30 rounded-lg">Create</button>
                    <button onClick={() => setIsCreating(false)} className="px-4 py-2 hover:bg-white/5 text-gray-400 rounded-lg">Cancel</button>
                </div>
            )}

            {/* Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {profiles.map(profile => {
                    const isActive = profile === currentProfile;
                    return (
                        <div
                            key={profile}
                            onClick={() => handleSwitch(profile)}
                            className={clsx(
                                "group bg-surface border rounded-xl p-6 cursor-pointer transition-all relative overflow-hidden",
                                isActive
                                    ? "border-primary bg-primary/5 shadow-xl shadow-primary/10 ring-1 ring-primary"
                                    : "border-white/5 hover:border-white/20 hover:bg-white/5"
                            )}
                        >
                            <div className="flex items-center justify-between relative z-10">
                                <div className="flex items-center gap-4">
                                    <div className={clsx(
                                        "w-12 h-12 rounded-full flex items-center justify-center transition-colors",
                                        isActive ? "bg-primary text-white" : "bg-white/5 text-gray-500 group-hover:bg-white/10 group-hover:text-gray-300"
                                    )}>
                                        <User size={24} />
                                    </div>
                                    <div>
                                        <h3 className={clsx("font-bold text-lg", isActive ? "text-white" : "text-gray-300")}>
                                            {profile}
                                        </h3>
                                        <p className="text-sm text-gray-500">
                                            {isActive ? "Active Profile" : "Click to switch"}
                                        </p>
                                    </div>
                                </div>

                                <div className="flex items-center gap-2">
                                    {isActive && <Check className="text-primary mr-2" />}

                                    {profile !== 'Default' && (
                                        <button
                                            onClick={(e) => handleDelete(profile, e)}
                                            className="p-2 text-gray-600 hover:text-red-400 hover:bg-red-500/10 rounded-lg transition-colors opacity-0 group-hover:opacity-100"
                                            title="Delete Profile"
                                        >
                                            <Trash2 size={18} />
                                        </button>
                                    )}
                                </div>
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
