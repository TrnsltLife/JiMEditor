# Basics
- Procedural generation is based on IProceduralGenerator interface 
  - Currently produces a single standalone Scenario but could later be changed to generate campaigns
  - Generator parameters depend on the generator instance and can be used to control the scenario input.

# Milestones and Tasks
- First playtesting
  - (DONE) Implement story generator basic skeleton that outputs a Scenario that can be clicked through in the Companion App
  - (DONE) Implement more interactions to StoryGenerator
  - (DONE) Implement first StoryTemplate to give flavor to the story (and fill in Objective details and StoryPoints with those)
  - (DONE) Implement Threat track generation
  - Implement monster activations and random terrain interactions
  - Lots of smaller TODO's that need to  be checked

- Improvements 
 - Implement generator parameter modification screen
 - Improve interaction with boardgame-elements (e.g. things not directly visible in the App)
 - Implement more StoryArchetypes (that can use the same templates than the old ones)
 - Implement side quests 
 - Implement even more StoryTemplates (with help of others?)
 - Take Scenario difficulty level in to account
 - Make starting level adjustable
 - Add rewards both in middle of scenario and in the end
 
- Far in the future
 - Split generator logic to also be part of the Companion App
 - Campaign generation with meaningful connection and progress between Scenarios
 - BattleMap Scenarios