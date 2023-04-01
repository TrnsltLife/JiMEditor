using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural   
{
    public class SimpleGeneratorParameters
    {
        #region Basic parameters
        /// <summary>
        /// Give string value to use fixed seed, give NULL or empty string to use random seed.
        /// </summary>
        public string Seed = "8900184"; //"2554742";    // "testing";

        /// <summary>
        /// The basic archetype of the Scenario we want to generate. Null if should use random
        /// </summary>
        public StoryArchetype.Type? StoryArchetype = null;

        /// <summary>
        /// The story template name used to fill the StoryArchetype stories. Null if should use random
        /// </summary>
        public string StoryTemplate = null;

        /// <summary>
        /// Include the VILLAINS_OF_ERIADOR expansion
        /// </summary>
        public bool Has_VILLAINS_OF_ERIADOR = false;

        /// <summary>
        /// Include the SHADOWED_PATHS expansion
        /// </summary>
        public bool Has_SHADOWED_PATHS = false;

        /// <summary>
        /// Include the DWELLERS_IN_DARKNESS expansion
        /// </summary>
        public bool Has_DWELLERS_IN_DARKNESS = false;

        /// <summary>
        /// Include the SPREADING_WAR expansion
        /// </summary>
        public bool Has_SPREADING_WAR = false;

        /// <summary>
        /// Include the SCOURGES_OF_THE_WASTES expansion
        /// </summary>
        public bool Has_SCOURGES_OF_THE_WASTES = false;

        // TODO: Story length or e.g. StoryPoint coun as parameter?

        #endregion
        #region Advanced parameters

        /// <summary>
        /// Miminum number of objectives in the MAIN STORY
        /// </summary>
        public int MinMainStoryObjectiveCount = 4;

        /// <summary>
        /// Maximum number of objectives in the MAIN STORY
        /// </summary>
        public int MaxMainStoryObjectiveCount = 6;

        /// <summary>
        /// Percentage change that branches occur on StoryPoint for each Objective. 0% means no branching at all.
        /// </summary>
        public int BranchingProbability = 0;  // TODO: too much branching? at least we should have an upper limit
                                               // TODO: if branching, we need to make sure that the last goal is not branched!
                                               // we ended up simultaneously killing monster and scouring which might be done in the wront order

        /// <summary>
        /// If branching, minimum number of branches to have
        /// </summary>
        public int BranchingMinBranches = 2;

        /// <summary>
        /// If branching, maximum number of branches to have
        /// </summary>
        public int BranchingMaxBranches = 3;

        public int MaxThreat = 60;

        public int ThreatIntervalMin = 8;

        public int ThreatIntervalMax = 12;

        public int ThreatDiffMonsterPoolMultiplier = 2;

        #endregion
        #region Debug parameters

        /// <summary>
        /// Tells whether the generator should stop after generating StoryPoints and before filling them with content
        /// </summary>
        public bool DebugSkipStoryPointsFillIn = false;

        #endregion
    }
}
