using System;
using System.Collections.Generic;
using System.Linq;

namespace JiME.Procedural.StoryElements
{
    /// <summary>
    /// Utility class that actually holds the logic for generating stories
    /// </summary>
    class StoryGenerator
    {
        private StoryArchetype _archetype;
        private StoryTemplate _template;
        private Random _random;
        private Func<string> _generateRandomId;

        private Chapter _startingTileSet;
        private BaseTile _startingTile;        

        public StoryGenerator(StoryArchetype archetype, StoryTemplate template, Random random, Func<string> generateRandomId)
        {
            _archetype = archetype;
            _template = template;
            _random = random;
            _generateRandomId = generateRandomId;
        }

        /// <summary>
        /// Fills in basic scenario level detais for flavor
        /// </summary>
        public void FillInScenarioDetails(Scenario s)
        {
            s.scenarioName = _template.Name; // TODO: do better here
            s.specialInstructions = "";
            s.introBookData = new TextBookData("ScenarioIntroboook")
            {
                pages = new List<string>() { "PLACEHOLDER INTROBOOK STUFF" }
            };
            s.threatNotUsed = true; // TODO: ???
            s.shadowFear = 2;
        }
        
        /// <summary>
        /// Fills in one particular phases StoryPoints and it's Objective inside the scenario
        /// Expects to get ALL phaseStoryPoints IN ORDER THAT THEY HAPPEN
        /// </summary>
        public void FillInPhaseStoryPoint(Scenario s, StoryPhase phase, IEnumerable<StoryPoint> phaseStoryPoints)
        {
            // See which phase we operate in
            var phaseInfo = GetPhaseInfo(phase);

            // Determine the location type the phase happens in            
            // TODO: here we should also take note that certain places are not available in all collections 
            var phaseLocation = GetRandomFromEnumerable(phaseInfo.TakesPlaceInOneOf);
            var primaryLocation = StoryLocation.GetLocation(phaseLocation);
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
                _startingTile = CreateRandomTileAndAddtoTileset(s, tileSet, primaryLocation, secondaryLocations, mustBeFromPrimary: true);
                _startingTile.isStartTile = true;

                // TODO: we should basically have random tiles except for the first one but isRandomTiles must not be set to the "Start" tileset
            }
            else
            {
                // Later phases just generate the tilese that appears when first main objective is revealed
                tileSet = new Chapter(phase.ToString() + " " + primaryLocation.Name);
                tileSet.triggeredBy = phaseStoryPoints.Where(sp => sp.PartOfMainQuest).First().StartTriggerName;
                tileSet.isRandomTiles = true;
                tileSet.isDynamic = true; // Add as dynamic since the fog of war overlaps the tiles otherwise
            }
            s.AddChapter(tileSet);
            
            // Create rest of tiles for the phase (max 5 tiles)
            var phaseTileCount = Math.Min(5, phaseStoryPoints.Count());
            while(tileSet.tileObserver.Count < phaseTileCount)
            {
                CreateRandomTileAndAddtoTileset(s, tileSet, primaryLocation, secondaryLocations, mustBeFromPrimary: false);
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
                    mainFragmentForStoryPoint = GetRandomFromEnumerable(phaseInfo.MustHaveOneOf);
                    mainFragmentForPhaseNotUsed = false;
                }
                else
                {
                    mainFragmentForStoryPoint = GetRandomFromEnumerable(phaseInfo.CanHaveSomeOf);
                }

                // Fetch out secondary fragments for the SP if it has more than one endTrigger
                var secondaryFragmentsForStoryPoint = GetRandomFromEnumerable(phaseInfo.CanHaveSomeOf, sp.EndTriggerNames.Count - 1);

                // Update the MAIN STORY objective information based on fragments
                InventObjective(s, sp.Objective, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation);

                // Determine which tile in the set is linked to this storypoint
                var randomPhaseTile = GetRandomFromEnumerable(allPhaseTiles);
                allPhaseTiles.Remove(randomPhaseTile);
                
                // Write out the MAIN STORY storypoint
                InventStory(s, 
                    sp.StartTriggerName, 
                    sp.EndTriggerNames, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation,
                    randomPhaseTile);    
            }
            
            // TODO: SIDE STORY points as well?
        }

        private void InventObjective(Scenario s, Objective o, string mainStoryPoint, IEnumerable<string> secondaryStoryPoints, string phaseLocation)
        {
            // Switch the dataName to help debugging
            var newDataName = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            if (s.objectiveName == o.dataName)
            {
                // Also update starting objective if that changes
                s.objectiveName = newDataName;
            }
            o.dataName = newDataName;

            // Fill in the actual objective details
            o.objectiveReminder = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            o.textBookData = new TextBookData() { pages = new List<string>() { mainStoryPoint.ToString() + " in " + phaseLocation.ToString() } };
            // TODO: name, texts etc. 
            // TODO: rewards etc.
        }

        private void InventStory(Scenario s, string startTrigger, List<string> endTriggers, string mainFragment, IEnumerable<string> secondaryFragments, string location, HexTile tile)
        {
            // TODO: handle multi-target stories better with more context, now we just do individual stories to each target (with different fragments)
            for(int i = 0; i < endTriggers.Count; i++)
            {
                // Select correct fragment
                var fragment = (i == 0) ? mainFragment : secondaryFragments.ToList()[i - 1];

                // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
                var dummySolution = new DialogInteraction(fragment.ToString() + " (" + location.ToString() + ")" + GenerateRandomNameSuffix());
                dummySolution.eventBookData = new TextBookData()
                {
                    pages = new List<string>() { "What do you want to do?" }
                };
                dummySolution.choice1 = "Trigger: " + endTriggers[i];
                dummySolution.c1Text = "You decided to go forward";
                dummySolution.c1Trigger = endTriggers[i];
                dummySolution.choice2 = ""; // This removes the option alltogether
                dummySolution.choice3 = ""; // This removes the option alltogether
                dummySolution.triggerName = startTrigger;
                dummySolution.isTokenInteraction = true; // Needs to be token interaction so that it can be triggered by a token
                //dummySolution.tokenType = TokenType.Person;
                //dummySolution.personType = PersonType.Elf;
                s.AddInteraction(dummySolution);

                
                var token = new Token(TokenType.Search);
                // TODO:token position? 
                token.triggeredByName = startTrigger; // Only reveal the token when the correct objective has been activated    
                token.triggerName = dummySolution.dataName; // Token triggers the dialog
                tile.tokenList.Add(token);
                
                // TODO: Do we need hardcoded fragment listing for different token types? or to create monsters etc.
            }
        }

        private StoryArchetype.StoryPhase GetPhaseInfo(StoryPhase phase)
        {
            switch(phase)
            {
                case StoryPhase.Start:
                    return _archetype.Start;
                case StoryPhase.Middle:
                    return _archetype.Middle;
                case StoryPhase.End:
                    return _archetype.End;
                default: throw new NotImplementedException();
            }
        }

        private T GetRandomFromEnumerable<T>(IEnumerable<T> enumerable)
        {
            var index = _random.Next(enumerable.Count());
            return enumerable.Skip(index).Take(1).Single();            
        }

        private IEnumerable<T> GetRandomFromEnumerable<T>(IEnumerable<T> enumerable, int count)
        {
            for(int i = 0; i < count; i++)
            {
                yield return GetRandomFromEnumerable(enumerable);
            }
        }

        private string GenerateRandomNameSuffix() => string.Format(" ({0})", Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Gets a random tile from this location but make sure that it is one of the available tiles and updates availableTileIds list
        /// </summary>
        private StoryLocation.TileInfo GetRandomTileInfo(Scenario s, StoryLocation location)
        {
            // Determine which ids are actually valid
            var validTiles = location.KnownTiles.Values
                .Where(t => s.globalTilePool.Contains(t.IdNumber))
                .ToList();
            if (validTiles.Count == 0)
            {
                Console.WriteLine("WARNING: Couldn not find available " + location.Name + " tile anymore!");
                return null;
            }

            // Then take one at random
            var tile = GetRandomFromEnumerable(validTiles);
            s.globalTilePool.Remove(tile.IdNumber);
            return tile;
        }

        /// <summary>
        /// Creates a new tile and adds it to the given Chapter 
        /// </summary>
        private BaseTile CreateRandomTileAndAddtoTileset(Scenario s, Chapter tileset, StoryLocation primaryLocation, IEnumerable<StoryLocation> secondaryLocations, bool mustBeFromPrimary = false)
        {
            // Gather tile info
            var tileInfo = GetRandomTileInfo(s, primaryLocation);
            if (tileInfo == null)
            {
                // Primary location did not yield valid tile
                if (mustBeFromPrimary)
                { 
                    throw new Exception("Primary location did not contain valid tile!");
                }

                // Test secondaries
                if (secondaryLocations != null)
                {
                    foreach (var secondaryLocation in secondaryLocations)
                    {
                        tileInfo = GetRandomTileInfo(s, secondaryLocation);
                        if (tileInfo != null)
                        {
                            break;
                        }
                    }
                }
                if (tileInfo == null)
                {
                    throw new Exception("Primary or Secondary locations did not contain valid tile!");
                }
            }

            // Create the tile itself and add the exploration token
            // TODO: what does skipBuild mean? if false then we get some coordinate exception, might be just for the "Start" chapter since others use random tiles
            var tile = new HexTile(tileInfo.IdNumber, skipBuild: true) 
            {
                tileSide = tileInfo.TileSide,
                flavorBookData = !(tileInfo.ExplorationTexts?.Count > 0) ? null : new TextBookData()
                {
                    pages = new List<string>() { GetRandomFromEnumerable(tileInfo.ExplorationTexts) }
                }
            };

            // Add the chapter and return created tile
            tileset.AddTile(tile);
            return tile;
        }

        public enum StoryPhase
        {
            Start,
            Middle,
            End
        }
    }
}
