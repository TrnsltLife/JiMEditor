using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural.SimpleGenerator
{
    class SimpleGeneratorParameters
    {
        /// <summary>
        /// Give string value to use fixed seed, give NULL or empty string to use random seed.
        /// </summary>
        public string Seed = "5157902";    // "testing2";

        /// <summary>
        /// Tells whether the generator should stop after generating StoryPoints and before filling them with content
        /// </summary>
        public bool DebugSkipStoryPointsFillIn = true;

        public int MinObjectiveCount = 3;
        public int MaxObjectiveCount = 8;

        /// <summary>
        /// Percentage change that branches occur on each Objective. 0 means no branching at all.
        /// </summary>
        public int BranchingProbability = 20;
        public int BranchingMinBranches = 2;
        public int BranchingMaxBranches = 3;
    }
}
