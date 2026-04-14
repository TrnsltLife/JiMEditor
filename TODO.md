This is a list of things we need to do:

* App window: 
  * New Random Scenario should launch the random scenario editor
* Campaign window:
  * The Choose a Cover Image button should work. Create a way to save the image data as text like before. It should be the same format as it was before. This also needs to work for scenario images.
  * The Create Package button needs to work as before
  * Create and Add New Scenario button needs to work
  * Add Existing Scenario... button needs to work
  * Add Procedurally Generated Scenario... button should launch the random scenario editor
* Main  Window
  * Enable Events in the sidebar. Enable event types one by one.
  * App Version and Scenario Format Version in the status bar
* Settings Dialog TODO:
  *  Notes on intentionally deferred pieces
    *  Resolution editor actions (add/remove/edit) are present in layout but currently show:
      *  TODO-Avalonia-Reenable: Resolution editor flow is not ported yet.
    *  Intro editing is wired to existing StoryTextEditWindow as a practical interim.
    *  If you want, next I can port the full resolution editor workflow (the WPF TextEditorWindow-based behavior) so those three buttons become fully functional too.