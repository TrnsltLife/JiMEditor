This is a list of things we need to do:

* App window: 
  * New Random Scenario should launch the random scenario editor
* Campaign window:
  * The Create Package button needs to work as before
  * Create and Add New Scenario button needs to work
  * Add Existing Scenario... button needs to work
  * Add Procedurally Generated Scenario... button should launch the random scenario editor
* Main  Window
  * Enable Events in the sidebar. Enable event types one by one.
    x Conditional
    x Corruption
    x Decision
    x Dialog
    x Enemy
    x Item
    x Multi-Event
    x Persistent
    x Reward
    x Story Branch
    x Text Dialog
    x Title
    * Replace Token
    * Stat Test

  * App Version and Scenario Format Version in the status bar
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