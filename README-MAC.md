# Build JiMEditor.Avalonia on Mac

Short guide for building / publishing on macOS.

## Status

Mac MVP steps 1–7 are done: `net9.0`, no `UseWpf`, portable `Vector2`, Avalonia `MessageBox` / `FileDialogs`. The solution should **`dotnet build`** on Mac once the sibling `JiMEditor` assets repo is present.

Still deferred: signed/notarized `.app` bundling (see Avalonia macOS docs). Step 8 is Mac publish + smoke test.

## Prerequisites

1. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) for macOS (`dotnet --version` → 9.x).
2. Clone this repo **and** sibling [JiMEditor](../JiMEditor) side by side (assets are linked from `../JiMEditor/Assets`):

```
parent/
  JiMEditor/
  JiMEditor-Avalonia/   ← this repo
```

3. VS Code / Cursor: **C# Dev Kit** (or **C#**) extension.
4. Apple Silicon vs Intel: use RID `osx-arm64` (M1/M2/M3/…) or `osx-x64` (Intel).

## Visual Studio Code

1. Open the `JiMEditor-Avalonia` folder.
2. Restore + build (Terminal):

```bash
dotnet restore JiMEditor-Avalonia.sln
dotnet build src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj -c Debug
```

Or **Terminal → Run Build Task…** → `build-avalonia` (same task as Windows; works once the TFM is cross-platform).

3. Run / debug: **Run and Debug** → **JiMEditor.Avalonia** → Start (`F5`), after `launch.json` targets `net9.0`.

4. Or without the debugger:

```bash
dotnet run --project src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj
```

### Publish a Mac executable

Self-contained publish (includes .NET runtime). Prefer building **on a Mac** for the Mac RID you care about.

Apple Silicon:

```bash
dotnet publish src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj \
  -c Release -r osx-arm64 --self-contained true -p:UseAppHost=true
```

Intel Mac:

```bash
dotnet publish src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj \
  -c Release -r osx-x64 --self-contained true -p:UseAppHost=true
```

Output (after TFM is `net9.0`):

`src/JiMEditor.Avalonia/bin/Release/net9.0/osx-arm64/publish/`  
(or `…/osx-x64/publish/`)

Run the `JiMEditor.Avalonia` binary in that folder (chmod +x if needed).

Optional single-file:

```bash
dotnet publish src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj \
  -c Release -r osx-arm64 --self-contained true \
  -p:UseAppHost=true -p:PublishSingleFile=true
```

For a double-clickable **`.app` bundle**, signing, and notarization (Gatekeeper), see [Avalonia macOS deployment](https://docs.avaloniaui.net/docs/deployment/macos). A raw `publish` folder is enough for local testing.

## Cursor (Mac)

Yes — Cursor can build the same way as VS Code. It uses `.vscode/tasks.json` / `.vscode/launch.json` and the integrated terminal.

**No Cursor-specific scripts are required.** Use:

1. **Same as VS Code:** Build Task / `F5` / `dotnet build` / `dotnet publish` in the terminal.
2. **Agent / AI chat:** ask it to run those `dotnet` commands.
3. **Optional later:** add a `build.sh` / `run.sh` mirroring Windows `build.bat` / `run.bat` if you want one-liner convenience — not needed for Cursor itself.

Open **JiMEditor-Avalonia** as the workspace folder and enable the C# extension in Cursor if tasks/debug do not show up.
