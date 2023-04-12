using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Models;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.GenerationLogic
{
    static class FragmentUtils
    {
        // TODO: some way to override token types from the StoryTemplate would be nice  

        public static void FillInObjective(ProceduralGeneratorContext ctx, Objective o, string mainStoryPoint, IEnumerable<string> secondaryStoryPoints, string phaseLocation)
        {
            // Switch the dataName to help debugging
            var newDataName = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            if (ctx.Scenario.objectiveName == o.dataName)
            {
                // Also update starting objective if that changes
                ctx.Scenario.objectiveName = newDataName;
            }
            o.dataName = newDataName;

            // Fill in the actual objective details
            var objectiveInfo = ctx.StoryTemplate.GenerateObjectiveInformation(ctx.Random, ctx.StoryArchetype.Archetype, mainStoryPoint, ctx.TemplateContext);
            o.objectiveReminder = objectiveInfo.Reminder;
            o.textBookData = GeneratorUtils.CreateTextBook(objectiveInfo.IntroText);
            // TODO: rewards etc.

            // Also check if this objective ends a scenario
            foreach (var resolution in ctx.Scenario.resolutionObserver.Where(x => x.triggerName == o.nextTrigger))
            {
                resolution.pages = new List<string>() { ctx.StoryTemplate.GenerateResolutionText(ctx.Random, ctx.StoryArchetype.Archetype, mainStoryPoint, ctx.TemplateContext) };
                // TODO: take scenario success true/false in to account if we at some point have both resolutions (only after we actually create more than 1 success resolution)
                // TODO: handle resolution rewards
            }
        }

        public static void FillInStoryPoint(ProceduralGeneratorContext ctx, string startTrigger, List<string> endTriggers, string mainFragment, IEnumerable<string> secondaryFragments, string location, HexTile tile, StoryGenerator.StoryPhase phase)
        {
            for (int i = 0; i < endTriggers.Count; i++)
            {
                // Select correct fragment
                var fragment = (i == 0) ? mainFragment : secondaryFragments.ToList()[i - 1];
                var fragmentInfo = StoryFragment.GetFragment(fragment);

                // Randomly select interaction type for the fragment
                var interactionInfo = fragmentInfo.Interactions.GetRandomFromEnumerable(ctx.Random);
                switch (interactionInfo.Type)
                {
                    case InteractionType.Dialog:
                        CreateDialogInteraction(ctx, fragment, interactionInfo, startTrigger, endTriggers[i], location, tile, phase);
                        break;

                    case InteractionType.StatTest:
                        CreateStatTestInteraction(ctx, fragment, interactionInfo, startTrigger, endTriggers[i], location, tile, phase);
                        break;

                    case InteractionType.Threat:
                        CreateThreatInteraction(ctx, fragment, interactionInfo, startTrigger, endTriggers[i], location, tile, phase);
                        break;

                    default:
                        throw new Exception("FragmentUtils: Unhandled interaction type " + interactionInfo.Type.ToString());
                }
            }
        }

        #region Interaction - Dialog
        private static void CreateDialogInteraction(
            ProceduralGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var dialogInfo = ctx.StoryTemplate.GenerateDialogInteractionInfo(ctx.Random, fragmentName, ctx.TemplateContext);

                var dialog = new DialogInteraction("Dialog" + GenerateRandomNameSuffix());
                dialog.eventBookData = GeneratorUtils.CreateTextBook(dialogInfo.ActionText);

                // TODO: how to have more meaningful dialog options with different rewards etc? perhaps add additional reward interactions?
                // Note: "" removes the option altogether
                dialog.choice1 = dialogInfo.Choice1Text?.Length > 0 ? dialogInfo.Choice1Text : "";
                dialog.c1Text = dialogInfo.Result1Text;
                dialog.c1Trigger = dialogInfo.Choice1Triggers ? endTrigger : "None";
                dialog.choice2 = dialogInfo.Choice2Text?.Length > 0 ? dialogInfo.Choice2Text : "";
                dialog.c2Text = dialogInfo.Result2Text;
                dialog.c2Trigger = dialogInfo.Choice2Triggers ? endTrigger : "None";
                dialog.choice3 = dialogInfo.Choice3Text?.Length > 0 ? dialogInfo.Choice3Text : "";
                dialog.c3Text = dialogInfo.Result3Text;
                dialog.c3Trigger = dialogInfo.Choice3Triggers ? endTrigger : "None";

                if (!dialogInfo.Choice1Triggers && !dialogInfo.Choice2Triggers && !dialogInfo.Choice3Triggers)
                {
                    ctx.LogError(dialog.dataName + ": None of the dialog choices advance the story! See used StoryTemplate!");
                }

                // Handle what happens to the token
                if (dialogInfo.PersistentText?.Length > 0)
                {
                    // Persistent dialogs remain on the board after all options are exchausted and give the exhausted text
                    dialog.isPersistent = true;
                    dialog.persistentText = dialogInfo.PersistentText;
                }
                else
                {
                    // Non-persistent dialog token vanish from the board after all options are exhausted
                    dialog.isPersistent = false;
                }

                // Return for finalization
                return dialog;
            });
        }
        #endregion
        #region Interaction - StatTest
        private static void CreateStatTestInteraction(
            ProceduralGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var testInfo = ctx.StoryTemplate.GenerateStatTestInteractionInfo(ctx.Random, fragmentName, ctx.TemplateContext);

                var statTest = new TestInteraction("Test " + testInfo.StatHint.ToString() + GenerateRandomNameSuffix());

                // Setup basic test
                statTest.testAttribute = testInfo.StatHint;
                statTest.altTestAttribute = testInfo.AltStatHint ?? Ability.None;
                statTest.noAlternate = testInfo.AltStatHint == null;

                // Token interaction flavor text
                statTest.textBookData = GeneratorUtils.CreateTextBook(testInfo.TokenText);

                // Test main flavor text
                statTest.eventBookData = GeneratorUtils.CreateTextBook(testInfo.ActionText);

                // Setup test type
                bool failProgressesStory;
                if (testInfo.StatTestType == StoryTemplate.StatTestInteractionInfo.TypeEnum.OneTry)
                {
                    statTest.isCumulative = false;
                    statTest.passFail = false;
                    failProgressesStory = true;
                }
                else if (testInfo.StatTestType == StoryTemplate.StatTestInteractionInfo.TypeEnum.Retryable)
                {
                    statTest.isCumulative = true;
                    statTest.passFail = true;
                    failProgressesStory = false;
                }
                else // StoryTemplate.StatTestInteractionInfo.TypeEnum.Cumulative
                {
                    statTest.isCumulative = true;
                    statTest.passFail = false;
                    failProgressesStory = false;
                }

                // Success
                statTest.passBookData = GeneratorUtils.CreateTextBook(testInfo.SuccessText);
                statTest.successValue = testInfo.SuccessValue;
                statTest.successTrigger = endTrigger;
                statTest.rewardXP = 0;
                statTest.rewardLore = 0;
                statTest.rewardThreat = 5; // TODO: where to get a good value here?

                // Progress
                statTest.progressBookData = GeneratorUtils.CreateTextBook(testInfo.ProgressText);

                // Failure
                statTest.failBookData = GeneratorUtils.CreateTextBook(testInfo.FailureText);
                if (failProgressesStory)
                {
                    statTest.failTrigger = endTrigger; // Failure also progresses things for now, TODO: Perhaps add some more interactions if failed? e.g. have to fight a monster
                }                
                statTest.failThreat = 5; // TODO: where to get a good value here?
                
                return statTest;
            });
        }
        #endregion
        #region Interaction - Threat
        private static void CreateThreatInteraction(
            ProceduralGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            // TODO: scale difficulty etc. properly, or is that managed already by the system?

            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var threatTest = new ThreatInteraction("Threat!" + GenerateRandomNameSuffix());

                // Generate story flavor
                var storyInfo = ctx.StoryTemplate.GenerateThreatInteractionInfo(ctx.Random, fragmentName, ctx.TemplateContext);

                // Setup the specific ANTAGONIST monster if it is relevant here
                if (phase == StoryGenerator.StoryPhase.End && !ctx.MainAntagonistEncounterCreated)
                {
                    ctx.MainAntagonistEncounterCreated = true;

                    // Add main antagonist monster
                    var bossType = ctx.StoryTemplate.AntagonistMonsterIsOneOf_Filtered.GetRandomFromEnumerable(ctx.Random);                    
                    threatTest.AddMonster(new Monster((int)bossType)
                    {
                        dataName = ctx.TemplateContext.GetAntagonistName(),
                        isLarge = true,  // TODO: randomize these?
                        isArmored = true,
                        count = 1,
                        defaultStats = true, // TODO: bit enhanced stats?
                        isEasy = true,
                        isNormal = true, // TODO: differences for different difficulty levels?
                        isHard = true
                    });

                    // Token interaction flavor text
                    threatTest.textBookData = GeneratorUtils.CreateTextBook(storyInfo.AntagonistTokenText);

                    // Test main flavor text
                    threatTest.eventBookData = GeneratorUtils.CreateTextBook(storyInfo.AntagonistActionText);
                }
                else
                {
                    // Token interaction flavor text
                    threatTest.textBookData = GeneratorUtils.CreateTextBook(storyInfo.NormalTokenText);

                    // Test main flavor text
                    threatTest.eventBookData = GeneratorUtils.CreateTextBook(storyInfo.NormalActionText); 
                }

                // Setup filler enemies
                threatTest.difficultyBias = DifficultyBias.Medium;
                threatTest.basePoolPoints = 10;
                threatTest.includedEnemies = GeneratorUtils.PrepareIncludedMonsters(ctx.StoryTemplate.AntagonistHelperMonstersAreSomeOf_Filtered.ToArray());

                // Progress when threat has been defeated
                threatTest.triggerDefeatedName = endTrigger;

                return threatTest;
            });
        }
        #endregion
        #region Utils

        private static string GenerateRandomNameSuffix() => string.Format(" ({0})", Guid.NewGuid().GetHashCode());

        private static void AddTokenInteraction<TInteraction>(ProceduralGeneratorContext ctx, HexTile tile, string startTrigger, StoryFragment.InteractionInfo info, Func<TInteraction> setupAction) where TInteraction : InteractionBase
        {
            // Create the interaction with specific setup
            var i = setupAction();

            // Finalize the interaction
            i.triggerName = "None"; // We must not set a starting trigger since this is triggered by a Token
            i.isTokenInteraction = true; // Needs to be token interaction so that it can be triggered by a token
            i.tokenType = info.TokenHint ?? TokenType.Search;
            i.personType = ctx.BystanderPersonTokenType;
            ctx.Scenario.AddInteraction(i); // Add the trigger itself

            // Create token to trigger it
            var token = new Token(info.TokenHint ?? TokenType.Search, ctx.BystanderPersonTokenType);
            // TODO:token position? 
            token.triggeredByName = startTrigger; // Only reveal the token when the correct objective has been activated    
            token.triggerName = i.dataName; // Token triggers the dialog
            // TODO: token flavor text?
            tile.tokenList.Add(token);
        }

        #endregion
    }
}
