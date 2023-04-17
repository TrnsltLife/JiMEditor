using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural   
{
    public class ProceduralGeneratorParameters
    {
        #region Basic parameters
        /// <summary>
        /// Give string value to use fixed seed, give NULL or empty string to use random seed.
        /// </summary>
        public string Seed { get; set; } = null;

        /// <summary>
        /// The basic archetype of the Scenario we want to generate. Null if should use random
        /// </summary>
        public StoryArchetype.Type? StoryArchetype { get; set; } = null;

        /// <summary>
        /// The story template name used to fill the StoryArchetype stories. Null if should use random
        /// </summary>
        public string StoryTemplate { get; set; } = null;

        /// <summary>
        /// Include the VILLAINS_OF_ERIADOR expansion
        /// </summary>
        public bool Has_VILLAINS_OF_ERIADOR { get; set; } = false;

        /// <summary>
        /// Include the SHADOWED_PATHS expansion
        /// </summary>
        public bool Has_SHADOWED_PATHS { get; set; } = false;

        /// <summary>
        /// Include the DWELLERS_IN_DARKNESS expansion
        /// </summary>
        public bool Has_DWELLERS_IN_DARKNESS { get; set; } = false;

        /// <summary>
        /// Include the SPREADING_WAR expansion
        /// </summary>
        public bool Has_SPREADING_WAR { get; set; } = false;

        /// <summary>
        /// Include the SCOURGES_OF_THE_WASTES expansion
        /// </summary>
        public bool Has_SCOURGES_OF_THE_WASTES { get; set; } = false;

        // TODO: Story length or e.g. StoryPoint count as enumerated parameter, SHORT/MEDIUM/LONG?

        #endregion
        #region Advanced parameters

        /// <summary>
        /// Miminum number of objectives in the MAIN STORY
        /// </summary>
        public int MinMainStoryObjectiveCount { get; set; } = 4;

        /// <summary>
        /// Maximum number of objectives in the MAIN STORY
        /// </summary>
        public int MaxMainStoryObjectiveCount { get; set; } = 6;

        /// <summary>
        /// Percentage change that branches occur on StoryPoint for each Objective. 0% means no branching at all.
        /// </summary>
        public int BranchingProbability { get; set; } = 0;  // TODO: too much branching? at least we should have an upper limit
                                               // TODO: if branching, we need to make sure that the last goal is not branched!
                                               // we ended up simultaneously killing monster and scouring which might be done in the wront order

        /// <summary>
        /// If branching, minimum number of branches to have
        /// </summary>
        public int BranchingMinBranches { get; set; } = 2;

        /// <summary>
        /// If branching, maximum number of branches to have
        /// </summary>
        public int BranchingMaxBranches { get; set; } = 3;

        /// <summary>
        /// Maximum threat level where the scenario ends
        /// </summary>
        public int MaxThreat { get; set; } = 60;

        /// <summary>
        /// Minimum threat interval to next event
        /// </summary>
        public int ThreatIntervalMin { get; set; } = 8;

        /// <summary>
        /// Maximum threat interval to next event
        /// </summary>
        public int ThreatIntervalMax { get; set; } = 12;

        /// <summary>
        /// Multiplier to the monster pool size based on threat interval from last event. (e.g. Multiplier 2x and interval being from 24->31 means monster pool 14)
        /// </summary>
        public int ThreatDiffMonsterPoolMultiplier { get; set; } = 2;

        #endregion
        #region Debug parameters

        /// <summary>
        /// Tells whether the generator should stop after generating StoryPoints and before filling them with content to show StoryPoint structure.
        /// </summary>
        public bool DebugSkipStoryPointsFillIn = false;

        /// <summary>
        /// If true, story element check prints out much more detailed log.
        /// </summary>
        public bool DebugVerboseStoryElementCheck = false;

        #endregion
    }
}
