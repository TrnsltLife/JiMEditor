# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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