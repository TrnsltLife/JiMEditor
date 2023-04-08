using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.GenerationLogic
{
    /// <summary>
    /// Utility class that actually holds the logic for generating stories
    /// </summary>
    class StoryGenerator
    {
        private SimpleGeneratorContext _ctx;

        private Chapter _startingTileSet;
        private BaseTile _startingTile;        

        public StoryGenerator(SimpleGeneratorContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Fills in basic scenario level detais for flavor
        /// </summary>
        public void FillInScenarioDetails()
        {
            // Do some basic decisions
            _ctx.TemplateContext = _ctx.StoryTemplate.PrepareTemplateContext(_ctx.Random);
            _ctx.BystanderPersonTokenType = StoryTemplate.GetPersonType(_ctx.TemplateContext.SelectedBystanderRace);

            // Fill in basic scenario details
            _ctx.Scenario.scenarioName = _ctx.StoryTemplate.Name; // TODO: do better here?
            _ctx.Scenario.specialInstructions = "";
            _ctx.Scenario.introBookData = new TextBookData("ScenarioIntroboook")
            {
                pages = new List<string>() { _ctx.StoryTemplate.GenerateScenarioIntroduction(_ctx.Random, _ctx.StoryArchetype.Archetype, _ctx.TemplateContext) }
            };
            _ctx.Scenario.shadowFear = 2;

            // Implement threat levels
            GeneratorUtils.CalculateThreatValues(
                _ctx.Parameters.ThreatIntervalMin,
                _ctx.Parameters.ThreatIntervalMax,
                _ctx.Parameters.MaxThreat,
                _ctx.Random, (newThreat, threatDiff) =>
                {
                    var threatAction = new ThreatInteraction( "Threat Action: " + newThreat);
                    // TODO: limit antagonist helper types based on collections
                    threatAction.difficultyBias = DifficultyBias.Medium;
                    threatAction.basePoolPoints = _ctx.Parameters.ThreatDiffMonsterPoolMultiplier * threatDiff;
                    threatAction.includedEnemies = GeneratorUtils.PrepareIncludedMonsters(_ctx.StoryTemplate.AntagonistHelperMonstersAreSomeOf_Filtered.ToArray());
                    threatAction.eventBookData = GeneratorUtils.CreateTextBook(_ctx.StoryTemplate.GenerateThreatLevelIncreaseText(_ctx.Random, _ctx.TemplateContext));
                    _ctx.Scenario.AddInteraction(threatAction);

                    _ctx.Scenario.threatObserver.Add(new Threat("Threat Level: " + newThreat, thresholdValue: newThreat)
                    {
                        triggerName = threatAction.dataName
                    });
                });
            _ctx.Scenario.threatNotUsed = false;
            _ctx.Scenario.threatMax = _ctx.Parameters.MaxThreat;
        }
        
        /// <summary>
        /// Fills in one particular phases StoryPoints and it's Objective inside the scenario
        /// Expects to get ALL phaseStoryPoints IN ORDER THAT THEY HAPPEN
        /// </summary>
        public void FillInPhaseStoryPoint(StoryPhase phase, IEnumerable<StoryPoint> phaseStoryPoints)
        {
            // TODO: If single phase has multiple of same fragment, we should combine those in to one! Perhaps needs to be done even earlier in branching phase

            // See which phase we operate in
            var phaseInfo = GetPhaseInfo(phase);

            // Determine the location type the phase happens in        
            // TODO: here we could also take note that certain places are not available in all collections 
            //       note that we already limit actual selected tiles based on which collections are available so this is just an optimization
            var phaseLocation = phaseInfo.TakesPlaceInOneOf.GetRandomFromEnumerable(_ctx.Random);
            var primaryLocation = StoryLocation.GetLocation(phaseLocation);
            _ctx.LogInfo("PHASE {0} LOCATION {1}", phase, primaryLocation.Name);
            var secondaryLocations = phaseInfo.TakesPlaceInOneOf
                .Where(l => l != primaryLocation.Name)
                .Select(l => StoryLocation.GetLocation(l))
                .ToList();

            // Create location for the phase
            Chapter tileSet;
            if (phase == StoryPhase.Start)
            {
                // Start phase is a bit different since here we need to do the initial location
                tileSet = new Chapter("Start");
                _startingTileSet = tileSet;                

                // Create the very first tile (must be from the primary location selected)
                _startingTile = LocationUtils.CreateRandomTileAndAddtoTileset(_ctx, tileSet, primaryLocation, secondaryLocations, mustBeFromPrimary: true);
                _startingTile.isStartTile = true;

                // TODO: if we ever take more than one tile to the first stage, we need to make sure that the fist non-random chapter only has single tile
            }
            else
            {
                // Later phases just generate the tilese that appears when first main objective is revealed
                tileSet = new Chapter(phase.ToString() + " " + primaryLocation.Name);
                tileSet.triggeredBy = phaseStoryPoints.Where(sp => sp.PartOfMainQuest).First().StartTriggerName;
                tileSet.isRandomTiles = true;
                tileSet.isDynamic = true; // Add as dynamic since the fog of war overlaps the tiles otherwise
            }
            _ctx.Scenario.AddChapter(tileSet);
            
            // Create rest of tiles for the phase (max 5 tiles)
            var phaseTileCount = Math.Min(5, phaseStoryPoints.Count());
            while(tileSet.tileObserver.Count < phaseTileCount)
            {
                LocationUtils.CreateRandomTileAndAddtoTileset(_ctx, tileSet, primaryLocation, secondaryLocations, mustBeFromPrimary: false);
            }
            var allPhaseTiles = tileSet.tileObserver.OfType<HexTile>().ToList(); // TODO: non-hex tile in some far future?
            
            // Work through all the MAIN STORY storypoints in the phase
            var phaseMainStoryPoints = phaseStoryPoints.Where(sp => sp.PartOfMainQuest).ToList();
            var mainFragmentForPhaseNotUsed = true;
            for (int i = 0; i < phaseMainStoryPoints.Count; i++)
            {
                var sp = phaseMainStoryPoints[i];
                var isLastStoryPointInPhase = i == phaseMainStoryPoints.Count - 1;

                // Determine which story fragment to use as main fragment for this StoryPoint
                // NOTE: Can only be the phases "MAIN" fragment if this is the last SP in the phase
                string mainFragmentForStoryPoint;
                if (isLastStoryPointInPhase && mainFragmentForPhaseNotUsed)
                {
                    mainFragmentForStoryPoint = phaseInfo.MustHaveOneOf.GetRandomFromEnumerable(_ctx.Random);
                    mainFragmentForPhaseNotUsed = false;
                }
                else
                {
                    mainFragmentForStoryPoint = phaseInfo.CanHaveSomeOf.GetRandomFromEnumerable(_ctx.Random);
                }

                // Fetch out secondary fragments for the SP if it has more than one endTrigger
                var secondaryFragmentsForStoryPoint = phaseInfo.CanHaveSomeOf.GetRandomFromEnumerable(_ctx.Random, sp.EndTriggerNames.Count - 1);

                // Update the MAIN STORY objective information based on fragments
                FragmentUtils.FillInObjective(_ctx, sp.Objective, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation);

                // Determine which tile in the set is linked to this storypoint
                if (allPhaseTiles.Count == 0)
                {
                    throw new Exception("Ran out of tiles for the phase! Limit number of objectives or add more collections.");
                }
                var randomPhaseTile = allPhaseTiles.GetRandomFromEnumerable(_ctx.Random);
                allPhaseTiles.Remove(randomPhaseTile);

                // Write out the MAIN STORY storypoint
                FragmentUtils.FillInStoryPoint(_ctx,
                    sp.StartTriggerName, 
                    sp.EndTriggerNames, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation,
                    randomPhaseTile,
                    phase);    
            }

            // TODO: SIDE STORY points as well?
        }

        private StoryArchetype.StoryPhase GetPhaseInfo(StoryPhase phase)
        {
            switch(phase)
            {
                case StoryPhase.Start:
                    return _ctx.StoryArchetype.Start;
                case StoryPhase.Middle:
                    return _ctx.StoryArchetype.Middle;
                case StoryPhase.End:
                    return _ctx.StoryArchetype.End;
                default: throw new NotImplementedException();
            }
        }      

        public enum StoryPhase
        {
            Start,
            Middle,
            End
        }
    }
}
