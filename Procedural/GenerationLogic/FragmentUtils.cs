using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Models;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.GenerationLogic
{
    static class FragmentUtils
    {
        public static void FillInObjective(SimpleGenerator.SimpleGeneratorContext ctx, Objective o, string mainStoryPoint, IEnumerable<string> secondaryStoryPoints, string phaseLocation)
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
            o.objectiveReminder = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            o.textBookData = new TextBookData() { pages = new List<string>() { mainStoryPoint.ToString() + " in " + phaseLocation.ToString() } };
            // TODO: name, texts etc. 
            // TODO: rewards etc.
        }

        public static void FillInStoryPoint(SimpleGenerator.SimpleGeneratorContext ctx, string startTrigger, List<string> endTriggers, string mainFragment, IEnumerable<string> secondaryFragments, string location, HexTile tile, StoryGenerator.StoryPhase phase)
        {
            // TODO: handle multi-target stories better with more context, now we just do individual stories to each target (with different fragments)
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
            SimpleGenerator.SimpleGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var dialog = new DialogInteraction("Dialog" + GenerateRandomNameSuffix());
                dialog.eventBookData = new TextBookData()
                {
                    pages = new List<string>() { "What do you want to do?" }
                };
                dialog.choice1 = "Trigger: " + endTrigger;
                dialog.c1Text = "You decided to go forward";
                dialog.c1Trigger = endTrigger;
                dialog.choice2 = ""; // This removes the option alltogether
                dialog.choice3 = ""; // This removes the option alltogether

                // Return for finalization
                return dialog;
            });
        }
        #endregion
        #region Interaction - StatTest
        private static void CreateStatTestInteraction(
            SimpleGenerator.SimpleGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff

            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var mainStat = info.StatHint ?? Ability.Random;
                var statTest = new TestInteraction("Test " + mainStat.ToString() + GenerateRandomNameSuffix());

                // Setup basic test
                statTest.testAttribute = mainStat;
                statTest.altTestAttribute = info.AltStatHint ?? Ability.Random;
                statTest.noAlternate = info.AltStatHint == null;

                // Token interaction flavor text
                statTest.textBookData = new TextBookData()
                {
                    pages = new List<string>() { "Are you sure you want to start testing?" }
                };
                // Test main flavor text
                statTest.eventBookData = new TextBookData()
                {
                    pages = new List<string>() { "This TEST will be very hard!" }
                };

                // Setup test type
                if (info.StatTestType == StoryFragment.StatTestType.OneTry)
                {
                    statTest.isCumulative = false;
                    statTest.passFail = false;
                }
                else if (info.StatTestType == StoryFragment.StatTestType.Retryable)
                {
                    statTest.isCumulative = true;
                    statTest.passFail = true;
                }
                else // StoryFragment.StatTestType.Cumulative
                {
                    statTest.isCumulative = true;
                    statTest.passFail = false;
                }

                // Success
                statTest.passBookData = new TextBookData()
                {
                    pages = new List<string>() { "Test SUCCESSFUL, threat decreases" }
                };
                statTest.successValue = 4; // TODO: where to get a good value here?
                statTest.successTrigger = endTrigger;
                statTest.rewardXP = 0;
                statTest.rewardLore = 0;
                statTest.rewardThreat = 5; // TODO: where to get a good value here?

                // Progress
                statTest.progressBookData = new TextBookData()
                {
                    pages = new List<string>() { "Keep trying!" }
                }; ;

                // Failure
                statTest.failBookData = new TextBookData()
                {
                    pages = new List<string>() { "Test FAILED, threat increases" }
                }; ;
                statTest.failTrigger = endTrigger; // Failure also progresses things for now, TODO: Perhaps add some more interactions if failed? e.g. have to fight a monster
                statTest.failThreat = 5; // TODO: where to get a good value here?
                
                return statTest;
            });
        }
        #endregion
        #region Interaction - Threat
        private static void CreateThreatInteraction(
            SimpleGenerator.SimpleGeneratorContext ctx, 
            string fragmentName,
            StoryFragment.InteractionInfo info,
            string startTrigger,
            string endTrigger,
            string location,
            HexTile tile,
            StoryGenerator.StoryPhase phase)
        {
            // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
            // TODO: scale difficulty etc. properly

            AddTokenInteraction(ctx, tile, startTrigger, info, () =>
            {
                var threatTest = new ThreatInteraction("Threat!" + GenerateRandomNameSuffix());

                // Token interaction flavor text
                threatTest.textBookData = new TextBookData()
                {
                    pages = new List<string>() { "Group of enemies are nearby" }
                };
                // Test main flavor text
                threatTest.eventBookData = new TextBookData()
                {
                    pages = new List<string>() { "The enemies are attacking!" }
                };

                // Setup the specific ANTAGONIST monster if it is relevant here
                if (phase == StoryGenerator.StoryPhase.End && !ctx.MainAntagonistEncounterCreated)
                {
                    ctx.MainAntagonistEncounterCreated = true;

                    // Main antagonist type
                    var bossType = ctx.StoryTemplate.AntagonistMonsterIsOneOf.GetRandomFromEnumerable(ctx.Random);                    
                    threatTest.AddMonster(new Monster((int)bossType) // TODO: limit boss type based on collections
                    {
                        dataName = ctx.TemplateContext.GetAntagonistName(),
                        isLarge = true,
                        isFearsome = true, // TODO: randomize these?
                        isArmored = true,
                        count = 1,
                        defaultStats = true,
                        isEasy = true,
                        isNormal = true,
                        isHard = true
                    });
                }

                // Setup filler enemies
                // TODO: limit antagonist helper types based on collections
                threatTest.difficultyBias = DifficultyBias.Medium;
                threatTest.basePoolPoints = 10;
                threatTest.includedEnemies = PrepareIncludedMonsters(ctx.StoryTemplate.AntagonistHelperMonstersAreSomeOf.ToArray());

                // Progress when threat has been defeated
                threatTest.triggerDefeatedName = endTrigger;

                return threatTest;
            });
        }
        #endregion
        #region Utils

        private static string GenerateRandomNameSuffix() => string.Format(" ({0})", Guid.NewGuid().GetHashCode());

        private static void AddTokenInteraction<TInteraction>(SimpleGenerator.SimpleGeneratorContext ctx, HexTile tile, string startTrigger, StoryFragment.InteractionInfo info, Func<TInteraction> setupAction) where TInteraction : InteractionBase
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

        private static bool[] PrepareIncludedMonsters(params MonsterType[] monsters)
        {
            var includedEnemies = new bool[Collection.MONSTERS().Length].Fill(false);
            foreach(var monster in monsters)
            {
                includedEnemies[(int)monster] = true;
            }
            return includedEnemies;
        }

        #endregion
    }
}
