using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.GenerationLogic
{
    static class FragmentUtils
    {
        public static void FillInObjective(Scenario s, Objective o, string mainStoryPoint, IEnumerable<string> secondaryStoryPoints, string phaseLocation)
        {
            // Switch the dataName to help debugging
            var newDataName = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            if (s.objectiveName == o.dataName)
            {
                // Also update starting objective if that changes
                s.objectiveName = newDataName;
            }
            o.dataName = newDataName;

            // Fill in the actual objective details
            o.objectiveReminder = mainStoryPoint.ToString() + " in " + phaseLocation.ToString() + " (" + o.triggeredByName + ")";
            o.textBookData = new TextBookData() { pages = new List<string>() { mainStoryPoint.ToString() + " in " + phaseLocation.ToString() } };
            // TODO: name, texts etc. 
            // TODO: rewards etc.
        }

        public static void FillInStoryPoint(Scenario s, Random random, string startTrigger, List<string> endTriggers, string mainFragment, IEnumerable<string> secondaryFragments, string location, HexTile tile)
        {
            // TODO: handle multi-target stories better with more context, now we just do individual stories to each target (with different fragments)
            for (int i = 0; i < endTriggers.Count; i++)
            {
                // Select correct fragment
                var fragment = (i == 0) ? mainFragment : secondaryFragments.ToList()[i - 1];
                var fragmentInfo = StoryFragment.GetFragment(fragment);

                // Randomly select interaction type for the fragment
                var interactionInfo = fragmentInfo.Interactions.GetRandomFromEnumerable(random);
                switch (interactionInfo.Type)
                {
                    case InteractionType.Dialog:
                        CreateDialogInteraction(fragment, interactionInfo, s, random, startTrigger, endTriggers[i], location, tile);
                        break;

                    default:
                        throw new Exception("FragmentUtils: Unhandled interaction type " + interactionInfo.Type.ToString());
                }
            }
        }

        #region Interaction - Dialog
        private static void CreateDialogInteraction(
            string fragmentName,
            StoryFragment.InteractionInfo interactionInfo,
            Scenario s, 
            Random random, 
            string startTrigger, 
            string endTrigger, 
            string location, 
            HexTile tile)
        {
            // TODO: invent story elements from startTrigger to endTrigger based on fragment and other stuff
            var dummySolution = new DialogInteraction(fragmentName.ToString() + " (" + location.ToString() + ")" + GenerateRandomNameSuffix());
            dummySolution.eventBookData = new TextBookData()
            {
                pages = new List<string>() { "What do you want to do?" }
            };
            dummySolution.choice1 = "Trigger: " + endTrigger;
            dummySolution.c1Text = "You decided to go forward";
            dummySolution.c1Trigger = endTrigger;
            dummySolution.choice2 = ""; // This removes the option alltogether
            dummySolution.choice3 = ""; // This removes the option alltogether
                                        //dummySolution.triggerName = startTrigger; // This must not be set as it causes wrong triggers to fie in the App, this is triggered only by the token
            dummySolution.isTokenInteraction = true; // Needs to be token interaction so that it can be triggered by a token
                                                     //dummySolution.tokenType = TokenType.Person;
                                                     //dummySolution.personType = PersonType.Elf;
            s.AddInteraction(dummySolution);


            var token = new Token(TokenType.Search);
            // TODO:token position? 
            token.triggeredByName = startTrigger; // Only reveal the token when the correct objective has been activated    
            token.triggerName = dummySolution.dataName; // Token triggers the dialog
            tile.tokenList.Add(token);
        }
        #endregion
        #region Utils

        private static string GenerateRandomNameSuffix() => string.Format(" ({0})", Guid.NewGuid().GetHashCode());

        #endregion
    }
}
