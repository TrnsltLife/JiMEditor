using System;
using System.Collections.Generic;
using System.Linq;

namespace JiME.Procedural.SimpleGenerator
{
    class SimpleGenerator : IProceduralGenerator<SimpleGeneratorParameters>
    {
        public SimpleGeneratorParameters GetDefaultParameters() => new SimpleGeneratorParameters();

        public Scenario GenerateScenario(SimpleGeneratorParameters parameters)
        {
            // Set up the context for generating a Scenario
            var ctx = new SimpleGeneratorContext(parameters);

            // Step 1. Generate basic objective structure and StoryPoints to fill in
            GenerateObjectivesAndStoryPoints(ctx, visualizeStoryPointsInScenario: ctx.Parameters.DebugSkipStoryPointsFillIn);

            // Step 2. Fill in the story points with interactions
            if (!ctx.Parameters.DebugSkipStoryPointsFillIn)
            {
                // TODO: Not skipping this step -> fill actual storypoints
            }

            // Return finished scenario
            return ctx.Scenario;
        }

        private static void GenerateObjectivesAndStoryPoints(SimpleGeneratorContext ctx, bool visualizeStoryPointsInScenario = false)
        {
            // For now we just work towards single successful resolution
            AddResolution(ctx, true, "Scenario succeeded!");

            // Create objectives for the good resolutions by processing all triggers
            int objectiveCount = ctx.Random.Next(ctx.Parameters.MinObjectiveCount, ctx.Parameters.MaxObjectiveCount + 1);
            while(ctx.Scenario.objectiveObserver.Count <= objectiveCount)
            {
                AddObjective(ctx, "Objective");
            }

            // Now if we have just a single unconnected trigger (from objective can make that the starting objective)
            if (ctx.UnconnectedObjectiveTriggers.Count == 1)
            {
                // Just single unconnected -> use that as default
                var singleTrigger = ctx.UnconnectedObjectiveTriggers.Single();
                var singleObjective = ctx.Scenario.objectiveObserver
                    .Single(o => o.triggeredByName == singleTrigger);
                ctx.Scenario.objectiveName = singleObjective.dataName;

                // Also clear the trigger from the objective
                singleObjective.triggeredByName = "None";

                // Also find the story point for this objective and update that
                var storyPoint = ctx.StoryPoints.Single(sp => sp.Objective.GUID == singleObjective.GUID);
                storyPoint.ReplaceStartingTrigger("None");

                // Also remove the trigger for the objective since that is no longer needed
                var triggerToRemove = ctx.Scenario.triggersObserver
                    .Single(t => t.dataName == singleTrigger);
                ctx.Scenario.triggersObserver.Remove(triggerToRemove);
            }
            else
            {
                // Multiple unconnected objectives, need to create new objective to start them all

                // First fetch all the objectives that need updating
                var allUnconnectedTriggers = ctx.TakeAllObjectiveTriggers();
                var triggerNameToUse = allUnconnectedTriggers.First();
                var triggersNamesToRemove = allUnconnectedTriggers.Skip(1).ToList();

                // For unconnected objective triggers we simply use the same trigger to trigger all the objectives
                var unconnectedObjectivesToReLink = ctx.Scenario.objectiveObserver
                    .Where(o => triggersNamesToRemove.Contains(o.triggeredByName))
                    .ToList();
                foreach (var obj in unconnectedObjectivesToReLink)
                {
                    // Make them all use the same trigger
                    var originalTriggerName = obj.triggeredByName;
                    obj.triggeredByName = triggerNameToUse;

                    // Also find the story point for this objective and update that
                    var storyPoint = ctx.StoryPoints.Single(sp => sp.Objective.GUID == obj.GUID);
                    storyPoint.ReplaceStartingTrigger(triggerNameToUse);

                    // Also remove the unused trigger
                    var unusedTrigger = ctx.Scenario.triggersObserver.Single(t => t.dataName == originalTriggerName);
                    ctx.Scenario.triggersObserver.Remove(unusedTrigger);
                }

                // For unconnected conditionals we simply create a new StoryPoint that handles it
                // And make sure that the original trigger is no longer part of the conditional list
                var allUnconnectedConditionals = ctx.Scenario.interactionObserver
                    .OfType<ConditionalInteraction>()
                    .Where(c => c.triggerList.Any(ct => allUnconnectedTriggers.Contains(ct)))
                    .ToList();
                foreach (var conditional in allUnconnectedConditionals)
                {
                    // Find the objective this conditional points to
                    var obj = ctx.Scenario.objectiveObserver.Single(o => o.triggerName == conditional.finishedTrigger);

                    // Remove the new objective completion trigger from the original conditional trigger list
                    conditional.triggerList.Remove(triggerNameToUse);

                    // Find the items in this conditional that are in the unused list
                    var openEndpoints = conditional.triggerList
                        .Where(ct => triggersNamesToRemove.Contains(ct))
                        .ToList();
                    
                    // Create a StoryPoint between items (if there is anything on the list to connect to)
                    if (openEndpoints.Count > 0)
                    {
                        ctx.StoryPoints.Add(new StoryPoint(
                            obj, // For the objective coming after the conditional
                            triggerNameToUse, // Starting from the newly created objective
                            openEndpoints)); // To all the unused endpoints of the conditional
                    }
                }

                // Now everything uses the triggerNameToUse -> create new objective that triggers it
                var startingObjective = AddObjective(ctx, "start", 
                    onCompletionTriggerOverride: triggerNameToUse, 
                    createOpenedTrigger: false, // This also means StoryPoint is not created
                    forceNoBranching: true,
                    dataNameOverride: "start");
                ctx.Scenario.objectiveName = startingObjective.dataName;
            }

            // Debugging
            Console.WriteLine("OBJECTIVES: " + ctx.Scenario.objectiveObserver.Count);

            // Story point debugging
            foreach (var sp in ctx.StoryPoints)
            {
                // Print storypoint details
                Console.WriteLine(string.Format("StoryPoint: {0} -> {1} (Objective: {2})",
                sp.StartTriggerName,
                string.Format("[{0}]", string.Join(",", sp.EndTriggerNames)),
                sp.Objective.dataName));

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

            /*
            s.AddInteraction(new DecisionInteraction("What to do?")
            { 
                   isThreeChoices = false,
                   choice1 = "Complete objective",
                   choice1Trigger = "Complete objective",
                   choice2 = "Fail objective",
                   choice2Trigger = "End In Failure"
            });

            */
        }
        



        private static void AddResolution(SimpleGeneratorContext ctx, bool success, string text)
        {
            var triggerName = ctx.CreateNextTriggerId();

            // Create resolution
            var resolutionText = new TextBookData(success ? "Scenario Successful" : "Scenario Failed")
            {
                pages = new List<string>() { text },
                triggerName = triggerName
            };
            ctx.Scenario.AddResolution(resolutionText, success);

            // Create trigger for it
            ctx.Scenario.AddTrigger(triggerName);

            // Mark triggers that are needed for the resolutions
            ctx.UnconnectedObjectiveTriggers.Add(triggerName);
        }

        private static Objective AddObjective(SimpleGeneratorContext ctx, string text, string onCompletionTriggerOverride = null, bool createOpenedTrigger = true, bool forceNoBranching = false, string dataNameOverride = null)
        {
            // Handle triggers
            var objectiveOnCompletionTrigger = onCompletionTriggerOverride ?? ctx.TakeRandomObjectiveTrigger();
            var objectiveOpenedTrigger = "None";
            if (createOpenedTrigger)
            {
                // Only create trigger that opens this on request
                objectiveOpenedTrigger = ctx.CreateNextTriggerId();
                ctx.UnconnectedObjectiveTriggers.Add(objectiveOpenedTrigger);
                ctx.Scenario.AddTrigger(objectiveOpenedTrigger);
            }
            
            // Completion trigger for the objective is always added
            var objectiveCompletedTrigger = ctx.CreateNextTriggerId();
            //ctx.UnconnectedObjectiveTriggers.Add(objectiveCompletedTrigger); TODO: this should not be added here!
            ctx.Scenario.AddTrigger(objectiveCompletedTrigger);

            // Create the objective
            var objective = new Objective(dataNameOverride ?? objectiveOpenedTrigger)
            {
                triggeredByName = objectiveOpenedTrigger,
                triggerName = objectiveCompletedTrigger,
                nextTrigger = objectiveOnCompletionTrigger,
                textBookData = new TextBookData(objectiveOpenedTrigger)
                {
                    pages = new List<string>() { text }
                }
            };
            ctx.Scenario.AddObjective(objective);

            // Check if we branch here (requiring two or more objectives to complete)
            if (!forceNoBranching && ctx.RandomChance(ctx.Parameters.BranchingProbability))
            {
                // Do branching by introducing a ConditionalInteraction to before the objective
                var conditional = new ConditionalInteraction(ctx.CreateNextTriggerId());
                ctx.Scenario.AddInteraction(conditional);

                // Conditional is the one triggering the just created objective to complete (and it is no longer free)
                conditional.finishedTrigger = objective.triggerName;

                // Create random amount of new Objectives for the conditional
                var newObjectiveCount = ctx.Random.Next(ctx.Parameters.BranchingMinBranches, ctx.Parameters.BranchingMaxBranches + 1);
                for(int i = 0; i < newObjectiveCount; i++)
                {
                    var newTriggerId = ctx.CreateNextTriggerId();
                    ctx.UnconnectedObjectiveTriggers.Add(newTriggerId);
                    ctx.Scenario.AddTrigger(newTriggerId);
                    conditional.triggerList.Add(newTriggerId);
                    /*AddObjective(ctx, "BranchedObjective",
                        onCompletionTriggerOverride: newTriggerId,
                        createOpenedTrigger: true, // These we want to leave as unset
                        forceNoBranching: true, // <-- Or should we allow re-branching in here?
                        dataNameOverride: null); */
                }
                // In this case the StoryPoint will be added later if needed
            }
            else
            {
                // No branching, simply add StoryPoint to this single objective
                ctx.StoryPoints.Add(new StoryPoint(
                    objective, // Story for this Objective
                    objectiveOpenedTrigger, // From the point when the objective is revealed
                    new List<string>() { objectiveCompletedTrigger })); // To the point that completes the objective
            }

            return objective;
        }
    }
}
