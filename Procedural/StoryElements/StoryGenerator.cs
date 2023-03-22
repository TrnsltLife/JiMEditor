using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public StoryGenerator(StoryArchetype archetype, StoryTemplate template, Random random)
        {
            _archetype = archetype;
            _template = template;
            _random = random;
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
            var phaseLocation = GetRandomFromEnumerable(phaseInfo.TakesPlaceInOneOf);

            // TODO: where do we populate phase tiles?

            // Work through all the MAIN STORY storypoints in the phase
            var phaseMainStoryPoints = phaseStoryPoints.Where(sp => sp.PartOfMainQuest).ToList();
            var mainFragmentForPhaseNotUsed = true;
            for (int i = 0; i < phaseMainStoryPoints.Count; i++)
            {
                var sp = phaseMainStoryPoints[i];
                var isLastStoryPointInPhase = i == phaseMainStoryPoints.Count - 1;

                // Determine which story fragment to use as main fragment for this StoryPoint
                // NOTE: Can only be the phases "MAIN" fragment if this is the last SP in the phase
                StoryArchetype.StoryFragent mainFragmentForStoryPoint;
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

        private void InventObjective(Objective o, StoryArchetype.StoryFragent mainStoryPoint, IEnumerable<StoryArchetype.StoryFragent> secondaryStoryPoints, StoryArchetype.StoryLocation phaseLocation)
        {
            o.textBookData = new TextBookData() { pages = new List<string>() { mainStoryPoint.ToString() + " in " + phaseLocation.ToString() } };
            // TODO: name, texts etc. 
            // TODO: rewards etc.
        }

        private void InventStory(Scenario s, string startTrigger, List<string> endTriggers, StoryArchetype.StoryFragent mainFragment, IEnumerable<StoryArchetype.StoryFragent> secondaryFragments, StoryArchetype.StoryLocation location)
        {
            // TODO: handle multi-target stories better with more context, now we just do individual stories to each target (with different fragments)
            for(int i = 0; i < endTriggers.Count; i++)
            {
                // Select correct fragment
                var fragment = (i == 0) ? mainFragment : secondaryFragments.ToList()[i - 1];

                // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
                var dummySolution = new TextInteraction(fragment.ToString() + " in " + location.ToString());
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

        /// <summary>
        /// Gets a random element the enumerable
        /// </summary>
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

        public enum StoryPhase
        {
            Start,
            Middle,
            End
        }
    }
}
