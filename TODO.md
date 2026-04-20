This is a list of things we need to do:

* App window: 
  * New Random Scenario should launch the random scenario editor

* Campaign window:
  * The Create Package button needs to work as before
  * Create and Add New Scenario button needs to work
  * Add Existing Scenario... button needs to work
  * Add Procedurally Generated Scenario... button should launch the random scenario editor

* Main  Window
  * Enable New Campaign button
  * Enable Save button
    * Save should use similar logic to ScenarioCollectionRemovalImpact and remove any disallowed items base on currently selected Collections
  * Create Save As button?
  * Enable Load button
    * Have its dialog ask if you want to Save First, Open, or Cancel
  * Enemy Event isn't respecting allowed enemy types based on scenario collections selected
  * Graph capabilities ported - check if libraries support Mac/Linux:
    * to FlowGraph?
      * https://github.com/prismify-co/FlowGraph
    * to NodeEditor?
      * https://wieslawsoltes.github.io/NodeEditor/
    * to AvaloniaGraphControl?
      * https://github.com/Oaz/AvaloniaGraphControl

  * Graphic Hex Tile Editor
    * Journey flow: Tile Editor / Token Editor. Battle scenarios: `TileEditorWindow` opens with square tiles (998/999), 25px drag grid, diamond cell geometry; parity polish may still be needed vs WPF.
    * Graphic Primitives: https://docs.avaloniaui.net/docs/graphics-animation/drawing-graphics
    * Shapes & Geometry: https://docs.avaloniaui.net/docs/graphics-animation/shapes-and-geometries
    * Image Brush: https://docs.avaloniaui.net/docs/graphics-animation/brushes

* Settings Dialog TODO:
  *  Notes on intentionally deferred pieces
    *  Resolution editor actions (add/remove/edit) are present in layout but currently show:
      *  TODO-Avalonia-Reenable: Resolution editor flow is not ported yet.
    *  Intro editing is wired to existing StoryTextEditWindow as a practical interim.
    *  If you want, next I can port the full resolution editor workflow (the WPF TextEditorWindow-based behavior) so those three buttons become fully functional too.

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