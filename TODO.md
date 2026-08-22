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

* Scenario validity check
  * Click a button to check the Scenario
  * Check for missing/invalid properties
  * Check for missing links (triggers, events)
  * etc.
  * Show a dialog to list the problems
  * Add validity checker functions to each object type

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

* Campaign window: Done

* Settings Dialog TODO: Done

* Enemy Pool Simulation: Done

* Translations: Done

* Verification status (2026-04-19)
  * Journey tile/token `.jime` roundtrip numeric parity is covered by tests in `TileTokenSerializationRoundTripTests`.
  * Journey translation-tail parity now includes tests for chapter rename key migration and changed explored-text decertification via `ApplyTranslationUpdatesAfterEditorDialog`.
  * Event translation-tail parity: `EventEditorTranslationTailTests` covers `TextInteraction` and `DialogInteraction` rename-key migration and decertify-on-text-change (same post-dialog helper as `EventEditorFactory`).
  * Battle/square: first-pass `TileEditorWindow` + gallery (998/999) are wired; deep WPF parity (image scale, exact hit feel) may need follow-up.