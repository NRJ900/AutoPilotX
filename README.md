# AutoPilotX 🚀

A modern, high-performance Auto Clicker and Macro Recorder for Windows, built with .NET 9 and React (WebView2).

![Icons](app.ico)

## ✨ Features

*   **Advanced Auto Clicker**: High CPS supported, standard/double/triple clicks, and random interval jitter for human-like behavior.
*   **Macro Recorder**: Record mouse movements, clicks, keyboard inputs, and **scroll wheel** actions.
*   **Editing**: Fine-tune your macros with a visual timeline editor (Drag & Drop, Edit Delays).
*   **Profiles**: Create separate profiles for different games or workflows to keep your settings organized.
*   **Modern UI**: Beautiful dark-mode interface powered by React and Tailwind CSS.
*   **Overlay & Hotkeys**: Global hotkeys (F6, F9) to control playback from any application.
*   **Sound Effects**: Audio feedback for start/stop actions.
*   **Safe**: Built-in safety release for modifier keys (Win/Ctrl/Alt) to prevent stuck keys.

## 🛠️ Installation

### Quick Start
1.  Download the latest release ZIP.
2.  Extract the **entire folder** (do not separate `AutoPilotX.exe` from the `dist` folder).
3.  Run `AutoPilotX.exe`.

### Requirements
*   Windows 10 or Windows 11.
*   [.NET Desktop Runtime 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (If not self-contained).
*   [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (Usually pre-installed on Windows).

## 🎮 Controls

| Action | Default Hotkey | Description |
| :--- | :--- | :--- |
| **Toggle Auto Clicker** | `F6` | Starts or stops the clicking loop. |
| **Toggle Recording** | `F9` | Starts or stops recording a new macro. |
| **Play Macro** | `(Custom)` | Bind any macro to a specific hotkey in the settings. |

## 🏗️ Building from Source

**Prerequisites**:
*   Node.js & npm
*   .NET SDK 9.0

```powershell
# 1. Clone the repo
git clone https://github.com/NRJ900/AutoPilotX.git

# 2. Build Frontend
cd src/AutoPilotX.UI
npm install
npm run build

# 3. Build Backend
cd ../..
dotnet publish -c Release -r win-x64
```

## 📝 License
MIT License. Use responsibly.
