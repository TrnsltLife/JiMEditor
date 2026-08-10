# Build JiMEditor.Avalonia on Mac

Short guide for building / publishing on macOS.

## Status

Mac MVP code steps 1–7 are done (`net9.0`, no WPF). You can **build and cross-publish from Windows or Mac**. Running / smoke-testing the binary still needs a Mac (step 8). Signed `.app` / notarization is deferred.

## Prerequisites

1. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (`dotnet --version` → 9.x).
2. Clone this repo **and** sibling [JiMEditor](../JiMEditor) side by side (tile/token assets are linked from `../JiMEditor/Assets`):

```
parent/
  JiMEditor/
  JiMEditor-Avalonia/   ← this repo
```

3. VS Code / Cursor: **C# Dev Kit** (or **C#**) extension.
4. Apple Silicon vs Intel: RID `osx-arm64` (M1/M2/M3/…) or `osx-x64` (Intel).

## Visual Studio Code / Cursor

1. Open the `JiMEditor-Avalonia` folder.
2. Build:

```bash
./build.sh
# or: dotnet build JiMEditor-Avalonia.sln
```

Or **Terminal → Run Build Task…** → `build-avalonia`.

3. Run: `./run.sh` or **Run and Debug** → **JiMEditor.Avalonia** (`F5`).

## Publish a Mac executable

### Convenience scripts

From repo root (works on Mac, or cross-publish from Windows/Linux):

```bash
./publish-mac.sh            # default: osx-arm64
./publish-mac.sh osx-x64    # Intel Mac
```

Windows PowerShell:

```powershell
.\publish-mac.ps1
.\publish-mac.ps1 -Rid osx-x64
```

Output:

`src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/publish/`  
(or `…/osx-x64/publish/`)

On a Mac, run `./JiMEditor.Avalonia` from that folder (`chmod +x` if needed).

### Manual `dotnet publish`

```bash
dotnet publish src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj \
  -c Release -r osx-arm64 --self-contained true -p:UseAppHost=true
```

For a double-clickable **`.app` bundle**, signing, and notarization, see [Avalonia macOS deployment](https://docs.avaloniaui.net/docs/deployment/macos). A raw `publish` folder is enough for local testing.

## CI

GitHub Actions (`.github/workflows/build.yml`) builds/tests on Ubuntu and uploads an `osx-arm64` publish artifact (`jimeditor-avalonia-osx-arm64`). That artifact still needs a Mac to run.
