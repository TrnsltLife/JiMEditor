using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural
{
    public class SimpleGeneratorContext
    {
        public SimpleGeneratorParameters Parameters { get; private set; }
        public Scenario Scenario { get; private set; }
        public Random Random { get; private set; }

        public List<LogItem> GeneratorLogs { get; private set; } = new List<LogItem>();
        public bool HasErrors { get; private set; }

        private int _nextTriggerId = 1;

        /// <summary>
        /// List of story points we need to fill with content.
        /// Will be filled in backward order and mix and match MAIN and SIDE quest stories.
        /// </summary>
        public List<StoryPoint> AllStoryPoints { get; set; } = new List<StoryPoint>();

        public StoryArchetype StoryArchetype { get; set; }
        public StoryTemplate StoryTemplate { get; set; }

        public StoryTemplate.TemplateContext TemplateContext { get; set; }
        public PersonType BystanderPersonTokenType { get; set; }
        public bool MainAntagonistEncounterCreated { get; set; }

        public SimpleGeneratorContext(SimpleGeneratorParameters parameters)
        {
            Parameters = parameters;
            SetupScenario();

            // If we don't have the seed, create it randomly
            var seedString = parameters.Seed?.Length > 0
                ? parameters.Seed
                : (new Random().Next(10000000, 99999999)).ToString();
            LogInfo("RANDOM SEED STRING: " + seedString);

            // Setup Randomization based on Seed
            Random = new Random(seedString.GetHashCode());
        }

        /// <summary>
        /// Creates next unique trigger id
        /// </summary>
        public string CreateNextTriggerId() => (_nextTriggerId++).ToString();

        private void SetupScenario()
        {
            Scenario = new Scenario("SimpleScenario");
            Scenario.scenarioGUID = Guid.NewGuid();
            //Scenario.campaignGUID = Guid.NewGuid(); Not creating a campaign (yet).
            Scenario.scenarioTypeJourney = true;
            Scenario.projectType = ProjectType.Standalone;
            Scenario.fileVersion = Utils.formatVersion;
            Scenario.useTileGraphics = true;

            // Setup additional Collections based on parameters? CORE_SET is there by default
            if (Parameters.Has_VILLAINS_OF_ERIADOR)
            {
                Scenario.collectionObserver.Add(Models.Collection.VILLAINS_OF_ERIADOR);
            }
            if (Parameters.Has_SHADOWED_PATHS)
            {
                Scenario.collectionObserver.Add(Models.Collection.SHADOWED_PATHS);
            }
            if (Parameters.Has_DWELLERS_IN_DARKNESS)
            {
                Scenario.collectionObserver.Add(Models.Collection.DWELLERS_IN_DARKNESS);
            }
            if (Parameters.Has_SPREADING_WAR)
            {
                Scenario.collectionObserver.Add(Models.Collection.SPREADING_WAR);
            }
            if (Parameters.Has_SCOURGES_OF_THE_WASTES)
            {
                Scenario.collectionObserver.Add(Models.Collection.SCOURGES_OF_THE_WASTES);
            }
            Scenario.RefilterGlobalTilePool();

            // Add default stuff
            Scenario.triggersObserver.Add(Trigger.EmptyTrigger());
            Scenario.objectiveObserver.Add(Objective.EmptyObjective());
            Scenario.interactionObserver.Add(NoneInteraction.EmptyInteraction());
            foreach (DefaultActivations defAct in Utils.defaultActivations)
            {
                MonsterActivations act = new MonsterActivations(defAct);
                Scenario.activationsObserver.Add(act);
            }
            /*foreach (InteractionBase terrainInteraction in Utils.defaultTerrainInteractions)
            {
                if (!Scenario.interactionObserver.Contains(terrainInteraction))
                {
                    Scenario.interactionObserver.Add(terrainInteraction);
                }
            }*/

            // TODO: ???
            Scenario.wallTypes = new int[22];
            for (int i = 0; i < 22; i++)
                Scenario.wallTypes[i] = 0;//0=none, 1=wall, 2=river
        }
        
        public void LogInfo(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Info,
                Message = string.Format(msg, args)
            });
        }

        public void LogWarning(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Warning,
                Message = string.Format(msg, args)
            });
        }

        public void LogError(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Error,
                Message = string.Format(msg, args)
            });
            HasErrors = true;
        }

        public void ClearScenario()
        {
            Scenario = null;
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        public class LogItem
        {
            public LogType Type { get; set; }
            public string Message { get; set; }
        }
    }
}
