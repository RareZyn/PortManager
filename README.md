# ⚡ Port Killer

A sleek Windows desktop app for developers to monitor and kill processes running on common development ports — built with C# and WinForms.

![Port Killer Screenshot](Resources/PortKiller.png)

---

## Features

- **Live Port Monitoring** — Scans active TCP ports every 5 seconds automatically
- **One-Click Kill** — Kill any process by port with a single button click
- **Custom Ports** — Add any port number you want to track
- **Search & Filter** — Instantly filter ports by number, name, or process
- **Pre-loaded Common Ports** — Ships with ports for popular dev tools:
  - React / Next.js (3000), Vite (5173), Angular (4200)
  - ASP.NET / Flask (5000), Django (8000), Tomcat (8080)
  - MongoDB (27017), MySQL (3306), PostgreSQL (5432), Redis (6379)
  - Jupyter Notebook (8888), and more
- **Modern Dark UI** — Custom frameless window with gradient background, drag-to-move, and resize grip
- **Single EXE** — Ships as a self-contained executable, no .NET install required

---

## Quick Start — Put it on Your Desktop

### Option A: Download the EXE (Easiest)

1. Go to [Releases](../../releases)
2. Download `PortManager.exe`
3. Move it to your Desktop
4. Right-click → **Run as administrator**

> You only need the single `.exe` file — nothing else to install.

---

### Option B: Build it Yourself from Source

Follow these steps if you cloned the repo and want to build your own copy.

#### 1. Install the .NET 10 SDK

Download and install from: https://dotnet.microsoft.com/download

Verify it's installed:
```bash
dotnet --version
```
You should see `10.x.x`.

#### 2. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/port-killer.git
cd port-killer/PortManager
```

#### 3. Publish as a single EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

This produces a single file at:
```
publish\PortManager.exe
```

#### 4. Copy to your Desktop

```bash
copy publish\PortManager.exe "%USERPROFILE%\Desktop\PortManager.exe"
```

Or just drag and drop `publish\PortManager.exe` to your Desktop manually.

#### 5. Run it

Double-click `PortManager.exe` on your Desktop.

> **Tip:** Right-click → **Run as administrator** so the app can kill processes on protected ports.

---

## Requirements

- Windows 10/11 (x64)
- No .NET runtime required — everything is bundled inside the `.exe`
- Administrator rights recommended to kill certain processes

---

## How It Works

1. On launch, Port Killer runs `netstat -ano` to scan all active listening TCP ports
2. It matches found ports against the monitored list and shows which are in use
3. For each active port, it looks up the process name via its PID
4. Clicking **Kill** sends `Process.Kill()` to terminate the process tree
5. The list auto-refreshes every 5 seconds

---

## Project Structure

```
PortManager/
├── Models/
│   └── PortInfo.cs           # Port data model
├── UI/
│   ├── GlassPanel.cs         # Custom panel with glass effect
│   ├── PortCardControl.cs    # Individual port card UI component
│   ├── RoundedButton.cs      # Custom rounded button control
│   └── Theme.cs              # App-wide colors, fonts, and dimensions
├── Resources/
│   ├── PortKiller.png        # App icon (source)
│   ├── PortKiller.ico        # App icon (compiled into EXE)
│   └── app.manifest          # Windows UAC manifest (admin prompt)
├── MainForm.cs               # Main application window
├── PortService.cs            # Port scanning and process kill logic
├── Program.cs                # App entry point
└── PortManager.csproj
```

---

## Tech Stack

- **C# / .NET 10**
- **Windows Forms (WinForms)**
- Self-contained single-file publish (`PublishSingleFile=true`)
- No third-party NuGet packages

---

## License

MIT License — free to use, modify, and distribute.
