# Basics
- Procedural generation is based on IProceduralGenerator interface 
  - Currently produces a single standalone Scenario but could later be changed to generate campaigns
  - Generator parameters depend on the generator instance and can be used to control the scenario input.

# Milestones and Tasks
- Improvements 
 - Implement more StoryArchetypes (that can use the same templates than the old ones)
   - Check that StatTest all different options are hooked properly to events
   - KillAntagonist can only happen once, AntagonistRetreat cannot happen after KillAntagonist, similar rules to fragments?
   - Also need to mark certain interactions somehow to be part of the "main goal" of the scenario and have different texts, upate Threat-style as well
 - Balance out the generator (e.g. Threat track generation, also have more options than just monsters, e.g. nazgul cry -> fear damage)
   - ThreatLevelIncreasesTexts should perhaps be inside SupportedArchetypes and contain also more options
 - Implement StoryLocation specific random encounters (monters and other interactions) to fill in the world
 - Improve interaction with boardgame-elements (e.g. things not directly visible in the App) 
 - Implement side quests
 - Implement more StoryTemplates (with help of others?)
 - Take Scenario difficulty level in to account somehow
 - Make starting level adjustable
 - Add rewards both in middle of scenario and in the end
 
- Far in the future
 - Split generator logic to also be part of the Companion App (Requires porting Common, Models and Procedural to .Net Standard)
 - Campaign generation with meaningful connection and progress between Scenarios
 - BattleMap Scenarios