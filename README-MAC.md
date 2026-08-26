# Build JiMEditor.Avalonia on Mac

Guide for building, running, publishing, and packaging the Avalonia editor on macOS.

## Status

Mac MVP code steps 1–7 are done (`net9.0`, no WPF). You can **build and cross-publish from Windows or Mac**. Running / smoke-testing the binary still needs a Mac. **Code signing and notarization** are deferred (unsigned `.app` works for local use; Gatekeeper may require Right-click → Open the first time).

## Prerequisites

1. **.NET 9 SDK** — this repo targets `net9.0`. Older SDKs fail with `NETSDK1045` until 9.x is installed.
2. Clone this repo **and** sibling [JiMEditor](../JiMEditor) side by side (tile/token assets are linked from `../JiMEditor/Assets`):

```
parent/
  JiMEditor/
  JiMEditor-Avalonia/   ← this repo
```

3. VS Code / Cursor: **C# Dev Kit** (or **C#**) extension.
4. Apple Silicon vs Intel: RID `osx-arm64` (M1/M2/M3/…) or `osx-x64` (Intel).

### Install .NET 9 SDK (macOS)

```bash
dotnet --list-sdks
dotnet --version    # should report 9.x after install
```

**Option A — Homebrew:**

```bash
brew install --cask dotnet-sdk@9
```

**Option B — Official installer:** download **.NET SDK 9.0** for macOS from [dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0) and run the `.pkg`.

Open a **new terminal** after installing so `PATH` picks up the SDK.

---

## 1. Build

Compiles the solution. Does **not** produce a redistributable Mac app.

From the repo root:

```bash
./build.sh              # Debug (default)
./build.sh --release    # Release
```

Equivalent:

```bash
dotnet build JiMEditor-Avalonia.sln -c Debug
dotnet build JiMEditor-Avalonia.sln -c Release
```

Or in VS Code / Cursor: **Terminal → Run Build Task…** → `build-avalonia`.

You do **not** need `./build.sh --release` before publishing; `./publish-mac.sh` builds Release as part of publish.

---

## 2. Run (dev)

Runs the editor from source (builds if needed):

```bash
./run.sh              # Debug
./run.sh --release    # Release
```

Or **Run and Debug** → **JiMEditor.Avalonia** (`F5`).

---

## 3. Publish

Creates a **self-contained** Release build for a Mac architecture (includes the .NET runtime). Output is a folder of DLLs plus the `JiMEditor.Avalonia` native host.

By default, `./publish-mac.sh` also **packages** a `.app` (see [§4 Package](#4-package)). Use `--no-bundle` if you only want the publish folder.

```bash
./publish-mac.sh                 # osx-arm64 + Your Journey Editor.app
./publish-mac.sh osx-x64         # Intel Mac
./publish-mac.sh --no-bundle     # publish folder only (no .app)
./publish-mac.sh osx-x64 --no-bundle
```

Windows PowerShell (publish only; see `publish-mac.ps1` — bundling is via `bundle-mac-app.sh` on Unix/Mac):

```powershell
.\publish-mac.ps1
.\publish-mac.ps1 -Rid osx-x64
```

### Publish output

| RID | Folder |
|-----|--------|
| Apple Silicon (default) | `src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/publish/` |
| Intel | `src/JiMEditor.Avalonia/bin/Release/net9.0/osx-x64/publish/` |

Run from Terminal (keep the whole folder together):

```bash
cd src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/publish
./JiMEditor.Avalonia
```

Finder does **not** treat the bare `JiMEditor.Avalonia` binary as a double-clickable app — use the `.app` package below for that.

### Manual `dotnet publish`

```bash
dotnet publish src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj \
  -c Release -r osx-arm64 --self-contained true -p:UseAppHost=true
```

---

## 4. Package

Wraps the publish folder into a double-clickable **`Your Journey Editor.app`** (Avalonia [macOS deployment](https://docs.avaloniaui.net/docs/deployment/macos) layout: `Info.plist`, `Contents/MacOS/`, ring icon as `.icns`).

### Usual path (publish + package together)

```bash
./publish-mac.sh            # or: ./publish-mac.sh osx-x64
```

Produces:

```
src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/
  publish/                     # self-contained files
  Your Journey Editor.app/     # double-clickable bundle
```

Open it:

```bash
open "src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/Your Journey Editor.app"
```

Or double-click **Your Journey Editor** in Finder. First launch of an **unsigned** build may need **Right-click → Open**.

### Package only (re-wrap an existing publish)

If you already published (or used `--no-bundle`):

```bash
./bundle-mac-app.sh              # wraps osx-arm64/publish
./bundle-mac-app.sh osx-x64
./bundle-mac-app.sh /path/to/publish
```

### Packaging assets

| File | Role |
|------|------|
| `packaging/macos/Info.plist` | Bundle id, display name, executable name |
| `packaging/macos/ring.icns` | Finder / Dock icon (from `src/JiMEditor.Avalonia/ring.ico`) |
| `packaging/macos/make-ring-icns.py` | Regenerates `ring.icns` on a Mac (`sips` + `iconutil`) |

```bash
python3 packaging/macos/make-ring-icns.py
```

Signing, notarization, and DMG installers are not automated yet; see the Avalonia docs when you are ready to distribute outside your machine.

---

## Quick reference

| Goal | Command |
|------|---------|
| Compile Debug | `./build.sh` |
| Compile Release | `./build.sh --release` |
| Run from source | `./run.sh` or `./run.sh --release` |
| Publish + `.app` (Apple Silicon) | `./publish-mac.sh` |
| Publish + `.app` (Intel) | `./publish-mac.sh osx-x64` |
| Publish folder only | `./publish-mac.sh --no-bundle` |
| Re-package `.app` | `./bundle-mac-app.sh` |

---

## CI

GitHub Actions (`.github/workflows/build.yml`) builds/tests on Ubuntu and uploads an `osx-arm64` publish artifact (`jimeditor-avalonia-osx-arm64`). That artifact still needs a Mac to run.
