# Build JiMEditor.Avalonia on Windows

Short guide for producing a runnable app / executable on Windows.

## Prerequisites

1. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (`dotnet --version` should report 9.x).
2. Clone this repo **and** the sibling [JiMEditor](../JiMEditor) repo side by side (tile/token assets are linked from `../JiMEditor/Assets`).

```
parent/
  JiMEditor/
  JiMEditor-Avalonia/   ← this repo
```

3. VS Code / Cursor: install the **C# Dev Kit** (or **C#**) extension so Run/Debug and tasks work.

## Visual Studio Code

1. Open the `JiMEditor-Avalonia` folder (File → Open Folder).
2. Restore + build:
   - **Terminal → Run Build Task…** and choose `build-avalonia`, **or**
   - In the integrated terminal:

```powershell
dotnet restore JiMEditor-Avalonia.sln
dotnet build src\JiMEditor.Avalonia\JiMEditor.Avalonia.csproj -c Debug
```

3. Run / debug: **Run and Debug** → select **JiMEditor.Avalonia** → Start (`F5`).  
   That uses `.vscode/launch.json` (builds first via the `build-avalonia` task).

4. Or run without the debugger:

```powershell
dotnet run --project src\JiMEditor.Avalonia\JiMEditor.Avalonia.csproj
```

Convenience scripts (from repo root): `build.bat`, `run.bat`.

Cross-publish a Mac build from Windows (needs sibling `JiMEditor` assets):

```powershell
.\publish-mac.ps1              # osx-arm64
.\publish-mac.ps1 -Rid osx-x64
```

That produces a folder you can copy to a Mac for smoke testing; see [README-MAC.md](README-MAC.md).

### Publish a Windows executable

Self-contained folder you can copy elsewhere (includes .NET runtime):

```powershell
dotnet publish src\JiMEditor.Avalonia\JiMEditor.Avalonia.csproj -c Release -r win-x64 --self-contained true
```

Output:

`src\JiMEditor.Avalonia\bin\Release\net9.0\win-x64\publish\`

Run `JiMEditor.Avalonia.exe` from that folder. Use `-r win-arm64` on ARM Windows if needed.

Framework-dependent (smaller; requires .NET 9 Desktop runtime installed on the machine):

```powershell
dotnet publish src\JiMEditor.Avalonia\JiMEditor.Avalonia.csproj -c Release -r win-x64 --self-contained false
```

## Cursor (Windows)

Yes — you can build from Cursor the same way as VS Code. Cursor is VS Code–compatible and already picks up this repo’s `.vscode/tasks.json` and `.vscode/launch.json`.

**No extra scripts are required** for Cursor. Use either:

1. **Same as VS Code:** Run Build Task / `F5` / integrated terminal `dotnet` commands above.
2. **Agent / AI chat:** ask it to run `dotnet build` or `dotnet publish` in the terminal (it can execute those commands for you).
3. **Optional convenience:** keep using `build.bat` / `run.bat` from a Cursor terminal; they are not Cursor-specific.

If tasks or debug do not appear, open the **JiMEditor-Avalonia** folder as the workspace root (not only the parent multi-root folder without this repo selected), and ensure the C# extension is enabled in Cursor.
