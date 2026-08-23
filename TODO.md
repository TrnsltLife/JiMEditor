This is a list of things we need to do:
* Tile Gallery:
  * Needs to be taller by default so scrolling isn't required

* Main  Window:
  * Clicking the Save icon the first time seems to launch the Save dialog twice.
  * Item shouldn't show Flavor Text for Token Interaction when no Token is selected. Check other Event types also.

* Rich Text Editor (WYSIWYG)
  * Implement an actual rich text editor instead of the current HTML string and preview
  * https://github.com/cuikp/AvRichTextBox

* Dirty Tracking:
  * Implement "dirty" tracking across all dialogs. A lot of work.
  * `Scenario.isDirty` exists and the window title already shows `*` when it is true (`MainWindow.SyncTitleFromScenario`).
  * It is **not** a reliable “has unsaved edits” flag today. New scenarios start dirty. Save / Save As clear it via `TriggerTitleChange()`. After that, only a few paths set it true again (objective OK, trigger add/rename/delete, new resolution, tile-block reorder). Most editors (events, tiles, tokens, scenario settings, translations, etc.) never mark dirty.
  * WPF was the same or weaker: it almost never called `TriggerTitleChange(true)` after load, so the close prompt mainly caught unsaved *new* projects.
  * Prompt before closing the main editor (and before New/Open that would discard the current project) when dirty. Even with incomplete tracking this is useful; once more edit paths set dirty, the prompt covers them automatically.
  * Prompt copy can match WPF: “The Project has changes that haven't been saved. Are you sure you want to exit without saving?”
  * Increase `isDirty` coverage so the prompt actually fires after real edits (not only new projects / a handful of dialogs):
    * Prefer marking dirty in `Scenario` mutators so individual windows cannot forget: `AddInteraction`, `RemoveData`, `AddChapter`, `AddObjective`, `AddTrigger`, `AddResolution`, `AddActivations`, `AddMonsterModifier`, `AddTranslation` (chapter reorder already calls `TriggerTitleChange(true)`).
    * Also mark dirty on **in-place OK** that mutates an existing object without going through those add/remove methods (event editors, tile/token editors, scenario settings, threats, translations, activations, bonuses).
    * Events: create, edit, duplicate, import, delete (sidebar + token editor “new event”).
    * Tile blocks: add / remove / properties; tile editor; token editor; tile pool / random tiles.
    * Objectives: delete / duplicate (objective editor OK already marks dirty).
    * Triggers: duplicate (add / rename / delete already mark dirty).
    * Resolutions: edit / delete from scenario settings (toolbar “new resolution” already marks dirty).
    * Enemy attack groups and enemy bonuses: add, edit, delete, import.
    * Translations: add, edit item text, delete, import.
    * Scenario settings: name, collections, intro / special / cover, scout values, rewards, starting objective, etc.
    * Threats window and inline scenario-name edit on the main toolbar.
    * Do **not** mark dirty for export-only or read-only actions (export event, save image, graph pan/zoom, ErrorChecker).
    * After Save / Save As, keep clearing dirty via `TriggerTitleChange()`; Save Copy must leave dirty unchanged (already the case).

* Keyboard Shortcuts:
  * [x] Reimplement WPF `MyCommands` gestures on the main editor using the **OS primary modifier** (Ctrl on Windows/Linux, Command on Mac) via `EditorHotKeys` (`KeyModifiers.Control` vs `Meta`).
  * Text editor (`PseudoHtmlRichEditor`): Ctrl/⌘+B bold, Ctrl/⌘+I italic, Ctrl/⌘+Space clear formatting (WPF Clear button; underline was disabled in WPF). Ctrl/⌘+F toggles Icon font wrap. Tooltips show the OS modifier.
  * WPF set (wired on `MainWindow` tunnel KeyDown):
    * Ctrl/Cmd+N — New project (return to project list)
    * Ctrl/Cmd+O — Open project
    * Ctrl/Cmd+S — Save
    * Ctrl/Cmd+Alt+S — Save As
    * Alt+S — Scenario settings
    * Alt+C — New tile block
    * Alt+O — New objective
    * Alt+E — New event
    * Alt+T — New trigger
    * Alt+X — Exit (closes the main editor)
  * Hold Ctrl/⌘ in Campaign Manager still means permanently delete a scenario (was Ctrl-only).

* Starting Position event:
  * [x] Match WPF when the user tries to **edit**, **duplicate**, or **import** the Starting Position event (delete was already blocked). Copy lives in `StartInteractionEditorMessages`.

* Search
  * A search function could be implemented
  * Use the Translation collector as a base since it collects a lot of strings already
  * Add in non-translation strings like event names and trigger names
  * Add in the actual translated strings
  * Put it in a list of items that, when clicked on, open the corresponding window (and tab?)
  * A lot of work probably

* Graph capabilities ported - check if libraries support Mac/Linux:
  1. NodeEditor - Verified Windows/Mac/Linux/Web/Android!
    * https://github.com/wieslawsoltes/NodeEditor
  2. to FlowGraph - Better sample image of styling and color
    * https://github.com/prismify-co/FlowGraph
  3. to AvaloniaGraphControl?
    * https://github.com/Oaz/AvaloniaGraphControl

* Better graphing capabilities
  * Better algorithm to display more helpfully

* Scenario validity check (`ErrorChecker`)
  * Add a **green checkmark** button on the main window top command bar that launches `ErrorChecker.CheckErrors`.
  * Advisory only: **non-blocking** for Save / Save As / close. Never prevent saving because of warnings.
  * Show a dialog listing problems (duplicate names, missing/invalid properties, missing trigger/event links, token `triggerName` pointing at a non-token or missing event, Starting Position token type != Start, etc.).
  * `ErrorChecker` exists but is not invoked from the UI yet (`ErrorChecker.cs` TODO-Avalonia). Expand the checker as we add per-type validity functions.

* Mac executable (MVP — strip WPF, don’t rebuild editors)
  * Goal: `dotnet publish -r osx-arm64 --self-contained` produces a runnable Mac binary (~1–1.5 weeks focused work)
  * [x] Flip TFM `net9.0-windows` → `net9.0`; remove `UseWpf`; update tests TFM + `.vscode/launch.json`
  * [x] Delete/exclude dead WPF: `Commands.cs`, `JiME/Common/Converters.cs`, legacy `Tile.cs`, `WindowExtensions` / WPF `HandleWindow`
  * [x] Strip WPF visuals from `BaseTile` / `HexTile` / `SquareTile` / `Token`; keep data fields; skip-build default
  * [x] Replace `System.Windows.Vector` / `Point` with portable `Vector2` (keep `X`/`Y` for `.jime`); update editors + round-trip tests
  * [x] Stop WPF image preload in `Utils.Init`; gallery uses `tileGalleryCount` + on-demand Avalonia bitmaps
  * [x] One dialog helper → replace ~25 `System.Windows.MessageBox` call sites (`JiME/Common/MessageBox.cs`)
  * [x] Win32 Open/Save → Avalonia `StorageProvider` (`FileDialogs` + FileManager/Import/Export/Campaign)
  * [x] Windows polish: deleted dead WPF files; `publish-mac.sh` / `.ps1`; CI build + osx-arm64 artifact; path hint + STA guard
  * Defer: model `Rehydrate`/mouse drag, graph polish, signed/notarized `.app` bundle
  * Next: step 8 — on a Mac, run published binary / smoke test (cross-publish already works from Windows)
  * See also: `README-MAC.md`, PLAN.md § Mac executable MVP

* App window: Done

* Campaign window: mostly done
  * [x] **Delete Campaign** — confirm, move scenarios back to the base folder (standalone + empty campaign GUID), delete the campaign folder (including package ZIP), return to the project list.

* Settings Dialog TODO: Done

* Enemy Pool Simulation: Done

* Translations: Done

* Verification status (2026-04-19)
  * Journey tile/token `.jime` roundtrip numeric parity is covered by tests in `TileTokenSerializationRoundTripTests`.
  * Journey translation-tail parity now includes tests for chapter rename key migration and changed explored-text decertification via `ApplyTranslationUpdatesAfterEditorDialog`.
  * Event translation-tail parity: `EventEditorTranslationTailTests` covers `TextInteraction` and `DialogInteraction` rename-key migration and decertify-on-text-change (same post-dialog helper as `EventEditorFactory`).
  * Battle/square: first-pass `TileEditorWindow` + gallery (998/999) are wired; deep WPF parity (image scale, exact hit feel) may need follow-up.