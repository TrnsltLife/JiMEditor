This is a list of things we need to do:

* App window: 
  * New Random Scenario should launch the random scenario editor

* Campaign window:
  * Add Procedurally Generated Scenario... button should launch the random scenario editor

* Rich Text Editor (WYSIWYG)
  * https://github.com/cuikp/AvRichTextBox

* Main  Window
  * Implement "dirty" tracking across all dialogs. A lot of work.

* Graph capabilities ported - check if libraries support Mac/Linux:
  1. NodeEditor - Verified Windows/Mac/Linux/Web/Android!
    * https://github.com/wieslawsoltes/NodeEditor
  2. to FlowGraph - Better sample image of styling and color
    * https://github.com/prismify-co/FlowGraph
  3. to AvaloniaGraphControl?
    * https://github.com/Oaz/AvaloniaGraphControl

* Settings Dialog TODO:
  *  Notes on intentionally deferred pieces
    *  Intro editing is wired to existing StoryTextEditWindow as a practical interim.

* Enemy Pool Simulation
  * Does not respect maximum enemy count. May have originally been a problem anyways.
  * In general seems to create too many enemies. What's the normal amount?
  * Ask AI to look at algorithm in Your Journey to align the simulator to Your Journey.

* Translations
  * I don't think all the Event editors are getting wired up for translations. Need to do this eventually.

* Verification status (2026-04-19)
  * Journey tile/token `.jime` roundtrip numeric parity is covered by tests in `TileTokenSerializationRoundTripTests`.
  * Journey translation-tail parity now includes tests for chapter rename key migration and changed explored-text decertification via `ApplyTranslationUpdatesAfterEditorDialog`.
  * Event translation-tail parity: `EventEditorTranslationTailTests` covers `TextInteraction` and `DialogInteraction` rename-key migration and decertify-on-text-change (same post-dialog helper as `EventEditorFactory`).
  * Battle/square: first-pass `TileEditorWindow` + gallery (998/999) are wired; deep WPF parity (image scale, exact hit feel) may need follow-up.