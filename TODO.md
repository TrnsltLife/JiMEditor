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
  * [x] `Scenario.MarkDirty()` + `PauseDirtyTracking()` for programmatic hydrate/sort.
  * [x] Close / New / Open / Return to Campaign prompt when dirty (WPF copy: “The Project has changes that haven't been saved. Are you sure you want to exit without saving?”).
  * [x] CollectionChanged tracker (`ScenarioDirty.Attach` from MainWindow): observers including `translationObserver` and nested `chapter.tileObserver` / `tokenList`. Does not listen to `filteredGlobalTilePool`.
  * [x] Input walker `ScenarioDirty.WatchInputs` on event dialogs, scenario/objective/threats/token/chapter/activations/modifier/translation/resolution/story/cover/enemy group/trigger/tile pool/tile editor. Skip Help, Error Check, Gallery paging. `Classes="ignoreDirty"` to opt out.
  * [x] Tile/token canvas drag, nudge, and rotate call `MarkDirty()`. Toolbar scenario-name commit marks dirty when the name changes.
  * Coarse is OK: Cancel after typing may still leave dirty. No undo / no “restored original text” comparison.
  * Save / Save As still clear dirty via `TriggerTitleChange()`. Save Copy leaves it unchanged.
  * Do **not** mark dirty for export-only or graph pan/zoom. Error Check prune still uses `TriggerTitleChange(true)`.
  * Leftover gaps (obscure editors) can be filled with the same `WatchInputs` one-liner if they show up in use.

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
  * [x] Green checkmark **Error Check** button on the main command bar (right of Save) runs `ErrorChecker.CheckErrors` and opens a read-only, selectable/copyable report dialog. Advisory only: does not block Save / Save As / close.
  * Checks today: duplicate names (same type only), missing/invalid event and trigger refs, token links, empty tile blocks, default player-facing placeholder text, and components from Collections not selected in Settings (tiles, tokens, monsters, items, mounts, titles).
  * Expand later with per-type validity (`CheckInteraction` is still a stub).

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