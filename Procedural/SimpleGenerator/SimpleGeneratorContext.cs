using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.SimpleGenerator
{
    class SimpleGeneratorContext
    {
        public SimpleGeneratorParameters Parameters { get; private set; }
        public Scenario Scenario { get; private set; }
        public Random Random { get; private set; }

        private int _nextTriggerId = 1;

        /// <summary>
        /// List of story points we need to fill with content.
        /// Will be filled in backward order and mix and match MAIN and SIDE quest stories.
        /// </summary>
        public List<StoryPoint> StoryPoints { get; set; } = new List<StoryPoint>();

        public SimpleGeneratorContext(SimpleGeneratorParameters parameters)
        {
            Parameters = parameters;
            SetupScenario();

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
        /// Calculates random change against given integer percentage.
        /// i.e. input 25 gives true approx 1/4 times
        public bool RandomChance(int probability)
        {
            var rnd = Random.Next(1, 100);
            return rnd < probability;
        }

        private void SetupScenario()
        {
            Scenario = new Scenario("SimpleScenario");
            Scenario.scenarioTypeJourney = true;
            Scenario.fileVersion = Utils.formatVersion;

            // TODO: setup additional Collections based on parameters? CORE_SET is there by default

            // Add default stuff
            Scenario.triggersObserver.Add(Trigger.EmptyTrigger());
            Scenario.objectiveObserver.Add(Objective.EmptyObjective());
            Scenario.interactionObserver.Add(NoneInteraction.EmptyInteraction());
            foreach (DefaultActivations defAct in Utils.defaultActivations)
            {
                MonsterActivations act = new MonsterActivations(defAct);
                Scenario.activationsObserver.Add(act);
            }

            // TODO: ???
            Scenario.wallTypes = new int[22];
            for (int i = 0; i < 22; i++)
                Scenario.wallTypes[i] = 0;//0=none, 1=wall, 2=river
        }
    }
}
