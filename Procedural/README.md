# Basics
- Procedural generation is based on IProceduralGenerator interface 
  - Currently produces a single standalone Scenario but could later be changed to generate campaigns
  - Generator parameters depend on the generator instance and can be used to control the scenario input.

# Milestones and Tasks
- Improvements 
 - Implement generator parameter modification screen
   - Advanced parameters
   - Generator log
   - (DONE) Able to use the generator in the campaign editor to create a random scenario (direct save is disabled)
 - Integrate visualizer to the actual editor window
   - Automatic update on change
   - Highlight on mouseover
 - Improve interaction with boardgame-elements (e.g. things not directly visible in the App)
 - Implement more StoryArchetypes (that can use the same templates than the old ones)
 - Implement side quests 
 - Implement even more StoryTemplates (with help of others?)
 - Take Scenario difficulty level in to account somehow
 - Make starting level adjustable
 - Add rewards both in middle of scenario and in the end
 
- Far in the future
 - Split generator logic to also be part of the Companion App
 - Campaign generation with meaningful connection and progress between Scenarios
 - BattleMap Scenarios