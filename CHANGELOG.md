# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added

### Changed

### Removed


## [v0,27]

### Added
- Notes to explain parts of UI that don't work as well as one would prefer - Translation Editor Window, Translation Item Editor Window, Enemy Activations Editor Window
- Changes to a scripted enemy's name decertifies the translation for that enemy name

## [v0.26]

### Added
- Callback on the AddEventPopup allows the originating screen to use a method to, e.g., select the created event in a Combo box

### Changed
- Use a CollectionViewSource to sort the Events, Trigges, Activations instead of running a sort on the underlying collection. This fixes a but with Triggers being unset by the next created Trigger.
- Fixed some duplicate name detection for Events and Activations

### Removed

## [v0.25]

### Added
- Added graph based visualization of the Scenario inside the main Scenario editor Window. 
  - The visualization shows all the major Scenario entities and trigger relations between them.
  - The visualization nodes are clickable to open the editor dialogs for the different entities
- Added EXPERIMENTAL procedural Scenario generator to the editor
  - Generator dialog can be opened either from the main Project screen or from Campaign Manager.
  - Generator can generate Scenarios based (as of now) two different StoryArchetypes: "OvercomingTheMonster" and "TheQuest"
  - Generator fills the selected StoryArchetype with a StoryTemplate, of which there is just one at the moment: "Orc Chieftain"
  - Generator dialog lets you fine tune the generator parameters and generates a new Scenario if you press the Die button
  - After generation, the dialog saves the used parameters and uses the same ones even after the editor is restarted (incl. the random seed if it was specified)
  - After generation, the dialog can visualize the Scenario for you by clicking the magnifying glass button (readonly view, for editing see below)
  - After generation, the dialog can save the generated Scenario directly to a file (unless opened from a Campaign Manager)
  - After generation, the dialog can "ACCEPT" the Scenario which either takes you to the main Scenario editor OR adds the Scenario to the Campaign
  - NOTE: This feature is EXPERIMENTAL and the generated Scenario -- while technically playable -- is very much unbalanced and unfinished at the moment.
    It can still be used for testing purposes or as a starting point for generating a Scenatio manually.
- Added "Skins" and "Languages" to the group of folders to exclude from treating as a Campaign folder.
- Added new text field to TokenTypeSelector, "Token Interaction Text", that allows custom text to display on token interaction button instead of Search, Threat, etc.
- Checkbox on events that can be part of a token interaction group (e.g. GRP1) to allow an event to be used more than once
- Added a Translation system, allowing you to translate the scenario text into any language you want.
  - You provide an ISO-639-2 or ISO-639-3 langauge code and language name.
  - You'll be presented with a list of all the texts that need to be translated and can translate them one by one.
  - Once you have translated an item, click the checkbox to mark it as finished. 
  - The editor keeps track of when you make changes to the fields that need translation and marks them as needing translation or review again.
  - The game app will allow players to select the languages you translate and show scenario text in that language.

### Changed
- Breaking Change: Any modifications to default enemy attack descriptions will not be saved. If you have changed enemy activations for default enemies, make a copy of your scenario file before you save with version 0.25 or your changes will be lost.
- Disabled the ability to modify the default enemy attack descriptions. Authors are required to make a copy in order to modify it.
- When a scenario is saved out, it only includes Monster Activations that are custom, not the default Activations
- Changed a lot of names in text and variable names to move away from names that are probably trademarked or could be.
- Both fixed and random tokens can be added to both fixed and random tile blocks

### Removed
- Removed enemy attack descriptions that aren't directly related to the enemy figurines. Relocated some enemy attack descriptions.
