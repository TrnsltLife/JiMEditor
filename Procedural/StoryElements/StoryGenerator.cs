﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Models;

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

        private IEnumerable<Collection> _availableCollections;
        private HashSet<int> _availableTiles;

        public StoryGenerator(StoryArchetype archetype, StoryTemplate template, Random random, IEnumerable<Collection> availableCollections)
        {
            _archetype = archetype;
            _template = template;
            _random = random;
            _availableCollections = availableCollections;
            _availableTiles = _availableCollections.SelectMany(c => c.TileNumbers).ToHashSet();
        }

        /// <summary>
        /// Fills in basic scenario level detais for flavor
        /// </summary>
        public void FillInScenarioDetails(Scenario s)
        {
            s.scenarioName = _template.Name; // TODO: do better here
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
            var phaseLocationInfo = StoryLocation.GetLocation(phaseLocation);
            
            // TODO: where do we populate phase tiles?
            var tile = GetRandomTile(phaseLocationInfo);

            // TODO: do we want to generate new terrain for each objective? or single one based on number of objetives in the phase?
            // TODO: what if we don't have enough e.g. village tiles? we could have FillInLocationsFrom in archetype and use secondary locations

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
                InventObjective(sp.Objective, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation);

                // Write out the MAIN STORY storypoint
                InventStory(s, 
                    sp.StartTriggerName, 
                    sp.EndTriggerNames, 
                    mainFragmentForStoryPoint, 
                    secondaryFragmentsForStoryPoint, 
                    phaseLocation);           
            }

            // TODO: SIDE STORY points as well?
        }

        private void InventObjective(Objective o, string mainStoryPoint, IEnumerable<string> secondaryStoryPoints, string phaseLocation)
        {
            o.textBookData = new TextBookData() { pages = new List<string>() { mainStoryPoint.ToString() + " in " + phaseLocation.ToString() } };
            // TODO: name, texts etc. 
            // TODO: rewards etc.
        }

        private void InventStory(Scenario s, string startTrigger, List<string> endTriggers, string mainFragment, IEnumerable<string> secondaryFragments, string location)
        {
            // TODO: handle multi-target stories better with more context, now we just do individual stories to each target (with different fragments)
            for(int i = 0; i < endTriggers.Count; i++)
            {
                // Select correct fragment
                var fragment = (i == 0) ? mainFragment : secondaryFragments.ToList()[i - 1];

                // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
                var dummySolution = new TextInteraction(fragment.ToString() + " (" + location.ToString() + ")" + GenerateRandomNameSuffix());
                dummySolution.triggerName = startTrigger;
                dummySolution.triggerAfterName = endTriggers[i];
                s.AddInteraction(dummySolution);
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
        private StoryLocation.TileInfo GetRandomTile(StoryLocation location)
        {
            // Determine which ids are actually valid
            var validTiles = location.KnownTiles.Values
                .Where(t => _availableTiles.Contains(t.IdNumber))
                .ToList();
            if (validTiles.Count == 0)
            {
                // TODO: or should we return an error here?
                throw new Exception("GetRandomTile() could not find any valid tiles!");
            }

            // Then take one at random
            var tile = GetRandomFromEnumerable(validTiles);
            _availableTiles.Remove(tile.IdNumber);
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
