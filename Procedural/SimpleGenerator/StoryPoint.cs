using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural.SimpleGenerator
{
    /// <summary>
    /// Placeholder for story element to be added towards some Objective
    /// </summary>
    class StoryPoint
    {
        /// <summary>
        /// Starting point of the story. Can in theory be NULL if this is the first point in the whole Scenario
        /// </summary>
        public string StartTriggerName { get; private set; }

        /// <summary>
        /// Ending points of the story. Can be single one OR multiple which ALL need to be completed before Objective is done.
        /// </summary>
        public List<string> EndTriggerNames { get; private set; }

        /// <summary>
        /// Objective of this Story
        /// </summary>
        public Objective Objective { get; private set; }

        public StoryPoint(Objective objective, string startTrigger, IEnumerable<string> endTriggers)
        {
            StartTriggerName = startTrigger;
            EndTriggerNames = endTriggers.ToList();
            Objective = objective;
        }

        public void ReplaceStartingTrigger(string newTrigger)
        {
            StartTriggerName = newTrigger;
        }
    }
}
