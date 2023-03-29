using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Procedural.GenerationLogic;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.SimpleGenerator
{
    /// <summary>
    /// Simple generator that generates a linear main objective chain and some possible side objectives
    /// </summary>
    class SimpleGenerator : IProceduralGenerator<SimpleGeneratorParameters>
    {
        public SimpleGeneratorParameters GetDefaultParameters() => new SimpleGeneratorParameters();

        public Scenario GenerateScenario(SimpleGeneratorParameters parameters)
        {
            // Set up the context for generating a Scenario
            var ctx = new SimpleGeneratorContext(parameters);

            // Step 1. Generate linear MAIN STORY objective structure and (possibly branching) StoryPoints to fill in later
            GenerateMainStoryObjectivesAndStoryPoints(ctx, visualizeStoryPointsInScenario: ctx.Parameters.DebugSkipStoryPointsFillIn);

            // TODO: Step 2. Generate side-quest objective structure and StoryPoints to fill in later

            // Step 3. Fill in the StoryPoints with interactions
            if (!ctx.Parameters.DebugSkipStoryPointsFillIn)
            {
                GenerateStoryTemplateAndFillInStoryPoints(ctx);
            }

            // Check errors and return finished scenario
            var checker = new ErrorChecker();
            checker.CheckErrors(ctx.Scenario);
            Console.WriteLine("SimpleGenerator ERROR: " + checker.Errors);

            // Return finished scenario
            return ctx.Scenario;
        }

        private static void GenerateMainStoryObjectivesAndStoryPoints(SimpleGeneratorContext ctx, bool visualizeStoryPointsInScenario = false)
        {
            // Everything we generate here is for the main quest
            var mainQuest = true;

            // For now we just work towards single successful resolution
            var resolution = AddResolution(ctx, true, "Scenario succeeded!");
            var nextHopTrigger = resolution.triggerName;

            // Create objectives back from the resolution up to desired objective count
            int objectiveCount = ctx.Random.Next(ctx.Parameters.MinMainStoryObjectiveCount, ctx.Parameters.MaxMainStoryObjectiveCount + 1);
            while(--objectiveCount >= 0)
            {
                if (objectiveCount == 0)
                {
                    // Creating the last objective
                    var obj = AddObjective(ctx, mainQuest, nextHopTrigger, "Objective",
                        createStartingObjective: true);
                    nextHopTrigger = null; // <-- This is not created for the starting object

                    // Also set this to appear on start of scenario
                    ctx.Scenario.objectiveName = obj.dataName;
                } 
                else
                {
                    // Creating intermediate objective
                    var obj = AddObjective(ctx, mainQuest, nextHopTrigger, "Objective",
                        createStartingObjective: false);
                    nextHopTrigger = obj.triggeredByName;
                }
            }

            // Debugging
            Console.WriteLine("MAIN OBJECTIVES: " + ctx.Scenario.objectiveObserver.Count);

            // Story point debugging
            foreach (var sp in ctx.AllStoryPoints)
            {
                // Print storypoint details
                Console.WriteLine(sp.ToString());

                // Visualization is done by adding dummy interaction that links the items
                // which alters the Scenario so should not be done if filling StoryPoints for real later
                if (visualizeStoryPointsInScenario)
                {   
                    var spInteraction = new StoryPointInteraction("STORYPOINT: " + sp.Objective.dataName)
                    {
                        triggerName = sp.StartTriggerName,
                        OtherAfterTriggers = sp.EndTriggerNames
                    };
                    ctx.Scenario.AddInteraction(spInteraction);
                }
            }
        }
        
        private static void GenerateStoryTemplateAndFillInStoryPoints(SimpleGeneratorContext ctx)
        {
            // Prepare StoryGenerator with StoryArchetype and StoryTemplate
            ctx.StoryArchetype = ctx.Parameters.StoryArchetype.HasValue
                ? StoryArchetype.GetArchetype(ctx.Parameters.StoryArchetype.Value)
                : StoryArchetype.GetRandomArchetype(ctx.Random);
            ctx.StoryTemplate = ctx.Parameters.StoryTemplate?.Length > 0
                ? StoryTemplate.GetTemplate(ctx.Parameters.StoryTemplate)
                : StoryTemplate.GetRandomTemplate(ctx.Random);
            var generator = new StoryGenerator(ctx);

            // Fill in the Scenario level details
            generator.FillInScenarioDetails();

            // Disect the MAIN STORY points (Note: storypoints are in reverse order
            var mainStoryPoints_StartPhase = new List<StoryPoint>();
            var mainStoryPoints_MiddlePhase = new List<StoryPoint>();
            var mainStoryPoints_EndPhase = new List<StoryPoint>();

            // We simple add single MAIN story point to both start and end and rest to middle
            var mainStoryPoints = ctx.AllStoryPoints.Where(sp => sp.PartOfMainQuest).Reverse().ToList();
            mainStoryPoints_StartPhase.Add(mainStoryPoints.First());
            mainStoryPoints_MiddlePhase.AddRange(mainStoryPoints.Skip(1).Take(mainStoryPoints.Count - 2));
            mainStoryPoints_EndPhase.Add(mainStoryPoints.Last());

            // TODO: Disect side quest storypoints somehow to the different phases

            // Fill in storypoints for each phase
            generator.FillInPhaseStoryPoint(StoryGenerator.StoryPhase.Start, mainStoryPoints_StartPhase);
            generator.FillInPhaseStoryPoint(StoryGenerator.StoryPhase.Middle, mainStoryPoints_MiddlePhase);
            generator.FillInPhaseStoryPoint(StoryGenerator.StoryPhase.End, mainStoryPoints_EndPhase);
        }


        private static TextBookData AddResolution(SimpleGeneratorContext ctx, bool success, string text)
        {
            // Create the trigger that triggers this resolution
            var triggerName = ctx.CreateNextTriggerId();

            // Create resolution
            var resolutionText = new TextBookData(success ? "Scenario Successful" : "Scenario Failed")
            {
                pages = new List<string>() { text },
                triggerName = triggerName
            };
            ctx.Scenario.AddResolution(resolutionText, success);

            // Create trigger for it
            ctx.Scenario.AddTrigger(triggerName, isMulti: true);

            return resolutionText;
        }

        /// <summary>
        /// Creates a new Objective and the matching StoryPoint for the scenario
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="mainQuest">Whether this objective is part of the linear main quest or not</param>
        /// <param name="onCompletionTrigger">Trigger that is fired when the objective completes</param>
        /// <param name="text">Dummy text for the objective</param>
        /// <param name="createStartingObjective">Whether to create the trigger that will trigger this objective, false if creating very first Objective for the scenario</param>
        /// <returns>The created Objective</returns>
        private static Objective AddObjective(SimpleGeneratorContext ctx, bool mainQuest, string onCompletionTrigger, string text, bool createStartingObjective = false)
        {
            // Handle opened trigger if requested
            var objectiveOpenedTrigger = "None";
            if (!createStartingObjective)
            {
                // Only create trigger that opens this on request
                objectiveOpenedTrigger = ctx.CreateNextTriggerId();
                ctx.Scenario.AddTrigger(objectiveOpenedTrigger, isMulti: true);
            }
            
            // Completion trigger for the objective is always added (but not added to unconnected as that only has the opened triggers)
            var objectiveCompletedTrigger = ctx.CreateNextTriggerId();
            ctx.Scenario.AddTrigger(objectiveCompletedTrigger, isMulti: true);

            // Create the objective
            var objective = new Objective(createStartingObjective ? "START" : "OBJECTIVE: " + objectiveOpenedTrigger)
            {
                triggeredByName = objectiveOpenedTrigger,
                triggerName = objectiveCompletedTrigger,
                nextTrigger = onCompletionTrigger,
                textBookData = new TextBookData(objectiveOpenedTrigger)
                {
                    pages = new List<string>() { text }
                }
            };
            ctx.Scenario.AddObjective(objective);

            // Check if we have just a simple StoryPoint or a more complex one
            if (!ctx.RandomChance(ctx.Parameters.BranchingProbability))
            {
                // No branching, simply add StoryPoint to this single objective to handle completing it
                ctx.AllStoryPoints.Add(new StoryPoint(
                    mainQuest,
                    objective, // Story for this Objective
                    objectiveOpenedTrigger, // From the point when the objective is revealed
                    new List<string>() { objectiveCompletedTrigger })); // To the point that completes the objective
            }
            else
            {
                // Do branching by introducing a ConditionalInteraction to before the objective
                var conditional = new ConditionalInteraction(ctx.CreateNextTriggerId());
                ctx.Scenario.AddInteraction(conditional);

                // Conditional is the one triggering the just created objective to complete (and it is no longer free)
                conditional.finishedTrigger = objectiveCompletedTrigger;

                // Create random amount of new Objectives for the conditional
                var storyPointEndTriggers = new List<string>();
                var branchCount = ctx.Random.Next(ctx.Parameters.BranchingMinBranches, ctx.Parameters.BranchingMaxBranches + 1);
                for(int i = 0; i < branchCount; i++)
                {
                    // Add new triggerpoint to the scenario
                    var newTriggerId = ctx.CreateNextTriggerId();
                    ctx.Scenario.AddTrigger(newTriggerId, isMulti: true);

                    // One which is triggered by the StoryPoint
                    storyPointEndTriggers.Add(newTriggerId);

                    // And whose triggering is required for the conditional and therefore the objective to complete
                    conditional.triggerList.Add(newTriggerId);                    
                }

                // Then add a StoryPoint to handle completing the objective
                var multiStoryPoint = new StoryPoint(
                    mainQuest,
                    objective, // Story for this Objective
                    objectiveOpenedTrigger, // From the point when the objective is revealed
                    storyPointEndTriggers); // The multiple points that complete the objective
                ctx.AllStoryPoints.Add(multiStoryPoint);
            }

            return objective;
        }
    }
}
