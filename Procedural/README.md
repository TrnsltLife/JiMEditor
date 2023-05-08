# Milestones and Tasks
- Improvements 
 - Ideas from actual gameplay
   - Threat track sometimes starts darkness at some point --> threat should have different progressions that are controlled by the template
   - Boss level enemies may need to be killed multiple times
 - Implement more StoryArchetypes (that can use the same templates than the old ones, perhaps with some archetype specific details)
   - Some fragments can be duplicates --> there should be at least 2-3 options for each objective in the StoryTemplate
   - Also need to mark certain interactions somehow to be part of the "main goal" of the scenario and have different texts, upate Threat-style as well
 - Balance out the generator (e.g. Threat track generation, also have more options than just monsters, e.g. nazgul cry -> fear damage)
   - ThreatLevelIncreasesTexts should perhaps be inside SupportedArchetypes and contain also more options
   - Also day turning in to night has been a valid threat progression so we might even implement more full control of this in the StoryTemplate
     (this basically means that the Threat track is also timing track and should be controlled by the Template)
 - Implement StoryLocation specific random encounters (monters and other interactions) to fill in the world
 - Multi-part StoryPoints especially in the decisive StoryPoints (e.g. antagonist is not defeated at one go, "cheat death" etc. )
 - Improve interaction with boardgame-elements (e.g. things not directly visible in the App, NEED TO KEEP Collections in mind with this!) 
 - Implement side quests (possibly using StoryBranchInteractions? e.g. FindWeaknessInAntagonist results in different branch and easier fight)
 - Implement more StoryTemplates (with help of others?)
 - Take Scenario difficulty level in to account somehow
 - Make starting level adjustable (Lore + XP, affects tiers of items etc. need to be selectable from few options?)
 - Add rewards (XP, Lore, Threat reward) both in middle of scenario and in the end
 
- Far in the future
 - Split generator logic to also be part of the Companion App (Requires porting Common, Models and Procedural to .Net Standard)	
 - Campaign generation with meaningful connection and progress between Scenarios
 - BattleMap Scenarios