using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural.StoryElements
{
    /// <summary>
    /// Helper class to go through all configured story elements and check for misconfigurations
    /// </summary>
    public static class StoryElementChecker
    {
        /// <summary>
        /// Logs in ERROR messages in case of FATAL problems. Those should be interpreted as "Cannot continue generation"
        /// </summary>
        public static void PerformCheck(LogContext ctx, bool verbose)
        {
            // Prepare helper that only logs if verbose is requested
            Action<string> logInfoVerbose = s => { if (verbose) { ctx.LogInfo(s); } };

            // First check that we have all the story Fragments and Locations as defined in the Story Archetypes
            ctx.LogInfo("Checking Story Archetypes, Fragments and Locations...");
            CheckCollection(logInfoVerbose, ctx, StoryArchetype.AllPossibleStoryFragments, StoryFragment.GetAllKnownFragmentNames(), "StoryFragment");
            CheckCollection(logInfoVerbose, ctx, StoryArchetype.AllPossibleStoryLocations, StoryLocation.GetAllKnownLocationNames(), "StoryLocation");
            
            // Then check that the templates meet all the requirements for the archetypes they support
            foreach (var t in StoryTemplate.GetAllKnownTemplates())
            {
                CheckTemplate(logInfoVerbose, ctx, t);
            }

            // Add an end note if there are ERROR's at this level
            if (ctx.HasErrors)
            {
                ctx.LogError("StoryElementCheck: Fatal errors encountered! Cannot continue generating a story.");
            }
        }

        private static void CheckCollection<T>(Action<string> logInfoVerbose, LogContext ctx, IEnumerable<T> allNeeded, IEnumerable<T> allKnown, string type)
        {
            logInfoVerbose("Checking known " + type + "s...");
            var allFound = true;
            foreach (var x in allNeeded)
            {
                if (allKnown.Contains(x))
                {
                    logInfoVerbose("- Found " + type + ": " + x);
                }
                else
                {
                    allFound = false;
                    ctx.LogError("- Could not find " + type + ": " + x);
                }
            }
            if (allFound)
            {
                logInfoVerbose("  --> All required " + type + "s were found... can continue.");
            }
            else
            {
                ctx.LogError("  --> Not all required " + type + "s were found... check configuration!");
            }

            // Then check if we have any extra story fragments that are not used
            var unused = allKnown.Except(allNeeded).ToList();
            if (unused.Count > 0)
            {
                ctx.LogWarning("Unused " + type + "s found");
                foreach (var x in unused)
                {
                    ctx.LogWarning("- " + x);
                }
            }
        }

        private static void CheckTemplate(Action<string> logInfoVerbose, LogContext ctx, string templateName)
        {
            ctx.LogInfo("Checking StoryTemplate: " + templateName);
            var template = StoryTemplate.GetTemplate(templateName);

            logInfoVerbose("  Checking supported Story Archetypes");
            foreach(var archetype in template.SupportedArchetypes.Keys)
            {
                var sa = template.SupportedArchetypes[archetype];

                // Check that we actually have intro texts for each archetype
                CheckListContainsAtLeastOne(logInfoVerbose, ctx, archetype, sa.ScenarioIntroductions, "Introduction");

                // Check which story fragments are needed by this archetype for the objectives
                var a = StoryArchetype.GetArchetype(archetype);
                
                var neededFragments = a.Start.MustHaveOneOf
                             .Union(a.Start.CanHaveSomeOf)
                             .Union(a.Middle.MustHaveOneOf)
                             .Union(a.Middle.CanHaveSomeOf)
                             .Union(a.End.MustHaveOneOf)
                             .Union(a.End.CanHaveSomeOf)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                logInfoVerbose("  - " + archetype.ToString() + " requires following fragments: " + string.Join(",", neededFragments));

                // Check that all the fragment needs have been net
                foreach (var neededFragment in neededFragments)
                {
                    CheckDictContainsAtLeastOne(logInfoVerbose, ctx, archetype, sa.ScenarioObjectives, neededFragment, "Objective");
                }

                // Check resolutions (At least one resolution text is required for each of the MustHave fragments of the Archetype End-phase)
                var neededResolutions = a.End.MustHaveOneOf;
                logInfoVerbose("  - " + archetype.ToString() + " requires following resolutions: " + string.Join(",", neededResolutions));
                foreach (var neededResolution in neededResolutions)
                {
                    CheckDictContainsAtLeastOne(logInfoVerbose, ctx, archetype, sa.Resolutions, neededResolution, "Resolution text");
                }
            }

            // Check that interactions have all been defined
            var neededDialogFragments = template.SupportedArchetypes.Values // From all the supported archetypes
                .SelectMany(x => x.ScenarioObjectives.Keys).Distinct() // Get the list of storyfragments
                .Where(x => StoryFragment.GetFragment(x).Interactions.Any(i => i.Type == InteractionType.Dialog)) // That require dialog interaction
                .ToList();
            logInfoVerbose("  - template requires following fragments for Dialog interaction: " + string.Join(",", neededDialogFragments));
            foreach (var neededDialogFragment in neededDialogFragments)
            {
                CheckDictContainsAtLeastOne(logInfoVerbose, ctx, null, template.DialogInteractions, neededDialogFragment, "DialogFragments");
            }

            // Check that interactions have all been defined
            var neededStatTestFragments = template.SupportedArchetypes.Values // From all the supported archetypes
                .SelectMany(x => x.ScenarioObjectives.Keys).Distinct() // Get the list of storyfragments
                .Where(x => StoryFragment.GetFragment(x).Interactions.Any(i => i.Type == InteractionType.StatTest)) // That require stat test interaction
                .ToList();
            logInfoVerbose("  - template requires following fragments for StatTest interaction: " + string.Join(",", neededStatTestFragments));
            foreach (var neededStatTestFragment in neededStatTestFragments)
            {
                CheckDictContainsAtLeastOne(logInfoVerbose, ctx, null, template.StatTestInteractions, neededStatTestFragment, "StatTestFragments");
            }

            // Check that interactions have all been defined
            CheckListContainsAtLeastOne(logInfoVerbose, ctx, null, template.ThreatInteractions, "ThreatInteraction");

            // TODO: All the other Story Template properties, text replace tokens, antagonist types etc.
        }

        private static void CheckListContainsAtLeastOne<T>(Action<string> logInfoVerbose, LogContext ctx, StoryArchetype.Type? archetype, IEnumerable<T> collection, string type)
        {
            if (collection?.Count() > 0)
            {
                logInfoVerbose("  - " + archetype.ToString() + " has " + collection.Count() + " " + type + "s");
            }
            else
            {
                ctx.LogError("  - {0} requires at least one " + type + " text", archetype);
            }
        }

        private static void CheckDictContainsAtLeastOne<T, V>(Action<string> logInfoVerbose, LogContext ctx, StoryArchetype.Type? archetype, Dictionary<T, List<V>> dictionary, T key, string type)
        {
            if (dictionary.ContainsKey(key) && dictionary[key].Count() > 0)
            {
                logInfoVerbose("  - " + key + " has " + dictionary[key].Count() + " defined " + type + "s");
            }
            else
            {
                ctx.LogError("  - {0} requires at least one " + type + " description for fragment: {1}", archetype, key);
            }
        }
    }

}
