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