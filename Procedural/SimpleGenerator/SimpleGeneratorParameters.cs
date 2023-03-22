using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.SimpleGenerator
{
    class SimpleGeneratorParameters
    {
        /// <summary>
        /// Give string value to use fixed seed, give NULL or empty string to use random seed.
        /// </summary>
        public string Seed = "testing2";    // "testing";

        /// <summary>
        /// The basic archetype of the Scenario we want to generate. Null if should use random
        /// </summary>
        public StoryArchetype.Type? StoryArchetype = null;

        /// <summary>
        /// The story template name used to fill the StoryArchetype stories. Null if should use random
        /// </summary>
        public string StoryTemplate = null;

        /// <summary>
        /// Miminum number of objectives in the MAIN STORY
        /// </summary>
        public int MinMainStoryObjectiveCount = 3;

        /// <summary>
        /// Maximum number of objectives in the MAIN STORY
        /// </summary>
        public int MaxMainStoryObjectiveCount = 6;

        /// <summary>
        /// Percentage change that branches occur on StoryPoint for each Objective. 0% means no branching at all.
        /// </summary>
        public int BranchingProbability = 20;

        /// <summary>
        /// If branching, minimum number of branches to have
        /// </summary>
        public int BranchingMinBranches = 2;

        /// <summary>
        /// If branching, maximum number of branches to have
        /// </summary>
        public int BranchingMaxBranches = 3;

        /// <summary>
        /// Tells whether the generator should stop after generating StoryPoints and before filling them with content
        /// </summary>
        public bool DebugSkipStoryPointsFillIn = false;
    }
}
