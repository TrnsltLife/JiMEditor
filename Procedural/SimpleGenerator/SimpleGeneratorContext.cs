using System;
using System.Collections.Generic;
using System.Linq;

namespace JiME.Procedural.SimpleGenerator
{
    class SimpleGeneratorContext
    {
        public SimpleGeneratorParameters Parameters { get; private set; }
        public Scenario Scenario { get; private set; }
        public Random Random { get; private set; }

        private int _nextTriggerId = 1;

        /// <summary>
        /// Keeps track of triggers for revealing objectives that haven't been used yet
        /// </summary>
        public List<string> UnconnectedObjectiveTriggers = new List<string>();

        /// <summary>
        /// List of story points we need to fill with content
        /// </summary>
        public List<StoryPoint> StoryPoints = new List<StoryPoint>();

        

        public SimpleGeneratorContext(SimpleGeneratorParameters parameters)
        {
            Parameters = parameters;
            Scenario = new Scenario("Simple Scenario");

            // If we don't have the seed, create it randomly
            var seedString = parameters.Seed?.Length > 0
                ? parameters.Seed
                : (new Random().Next(10000000)).ToString();
            Console.WriteLine("RANDOM SEED STRING: " + seedString);

            // Setup Randomization based on Seed
            Random = new Random(seedString.GetHashCode());
        }

        /// <summary>
        /// Creates next unique trigger id
        /// </summary>
        public string CreateNextTriggerId() => (_nextTriggerId++).ToString();

        /// <summary>
        /// Takes a single random trigger out of the set
        /// </summary>
        public string TakeRandomObjectiveTrigger()
        {
            var i = Random.Next(UnconnectedObjectiveTriggers.Count);
            var trigger = UnconnectedObjectiveTriggers[i];
            UnconnectedObjectiveTriggers.RemoveAt(i);
            return trigger;
        }

        /// <summary>
        /// Takes all remaining unconnected triggeres from the set
        /// </summary>
        public List<string> TakeAllObjectiveTriggers()
        {
            var all = UnconnectedObjectiveTriggers.ToList();
            UnconnectedObjectiveTriggers.Clear();
            return all;
        }

        /// <summary>
        /// Calculates random change against given integer percentage.
        /// i.e. input 25 gives true approx 1/4 times
        public bool RandomChance(int probability)
        {
            var rnd = Random.Next(1, 100);
            return rnd < probability;
        }
    }
}
