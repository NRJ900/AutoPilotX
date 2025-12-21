export interface MacroAction {
    Type: number;
    X: number;
    Y: number;
    Button: number;
    Key: string;
    DelayMs: number;
}

export interface Macro {
    Id: string;
    Name: string;
    Actions: MacroAction[];
    // Advanced
    RepeatMode: number; // 0=Inf, 1=Count
    RepeatCount: number;
    SpeedMultiplier: number;
    IsRelative: boolean;
}

export interface Bridge {
    StartAutoClicker(jsonSettings: string): void;
    StopAutoClicker(): void;
    IsAutoClickerRunning(): Promise<boolean>;
    PickLocation(): Promise<string>; // Fixed typo

    // Macros
    GetMacros(): Promise<string>; // Returns JSON string
    PrepareRecording(name: string, keys: boolean, mouse: boolean): void;
    ToggleRecording(): void;
    CancelRecording(): void;

    PlayMacro(name: string): void;
    StopMacro(): void;
    DeleteMacro(name: string): void;
    UpdateMacro(json: string): void;
    CreateMacro(name: string): void;

    // Hotkeys
    GetHotkeys(): Promise<string>;
    SetHotkey(id: string, key: number, modifiers: number): void;

    // Stats
    GetStats(): Promise<string>;

    // Profiles
    GetProfiles(): Promise<string>; // Returns JSON string array
    GetCurrentProfile(): Promise<string>;
    CreateProfile(name: string): void;
    SwitchProfile(name: string): void;
    DeleteProfile(name: string): void;

    // Settings
    GetSettings(): Promise<string>;
    SaveSettings(json: string): void;

    // Updates
    CheckUpdates(): Promise<string>;
    OpenUpdateUrl(url: string): void;

    // Window
    SetWindowMode(isMini: boolean): void;
    DragWindow(): void;
    GetDetailedStatus(): Promise<string>;
}

export interface AppSettings {
    AlwaysOnTop: boolean;
    MinimizeToTray: boolean;
    Theme: string;
    SoundEffects: boolean;
    SoundVolume: number;
    HumanLikeMouseMovement: boolean;
}

export interface HotkeyDefinition {
    Id: string;
    Description: string;
    Key: number;
    Modifiers: number;
}

declare global {
    interface Window {
        chrome: {
            webview: {
                hostObjects: {
                    bridge: Bridge;
                }
            }
        }
    }
}
