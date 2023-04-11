using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME.Procedural.StoryElements
{
    /// <summary>
    /// Specifies interactions and encounter types that the different parts of the story can entail
    /// </summary>
    class StoryFragment
    {
        #region Properties

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Comment { get; private set; }

        [JsonProperty]
        public List<InteractionInfo> Interactions { get; private set; }

        #endregion
        #region Construction and fetching
        private static Dictionary<string, StoryFragment> s_fragments;
        
        public static StoryFragment GetFragment(string name)
        {
            return s_fragments[name];
        }

        public static StoryFragment GetRandomFragment(Random r)
        {
            // Randomize archetype
            var possibleKeys = s_fragments.Keys.ToList();
            var templateKey = possibleKeys[r.Next(possibleKeys.Count)];

            // Get actual archetype
            return GetFragment(templateKey);
        }

        public static IEnumerable<string> GetAllKnownFragmentNames()
        {
            return s_fragments.Keys.ToList();
        }

        private StoryFragment() { /* Prevent construction from outside */ }

        static StoryFragment()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.Contains(".story-fragments.json"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var list = JObject.Parse(json);
                    s_fragments = list.SelectToken("fragments")
                        .ToObject<List<StoryFragment>>()
                        .ToDictionary(x => x.Name, x => x);

                    // Check what we have or have not from the archetypes list
                    var missingFragmentDefinitions = StoryArchetype.AllPossibleStoryFragments.Except(s_fragments.Keys).ToList();
                    var unusedFragmentDefinitions = s_fragments.Keys.Except(StoryArchetype.AllPossibleStoryFragments).ToList();

                    if (missingFragmentDefinitions.Count > 0)
                    {
                        Console.WriteLine("Story Fragments used in StoryArchetypes but NOT DEFINED:");
                        foreach (var f in missingFragmentDefinitions)
                        {
                            Console.WriteLine("  - " + f);
                        }
                    }

                    if (unusedFragmentDefinitions.Count > 0)
                    {
                        Console.WriteLine("Story Fragments defined here but NOT USED in StoryArchetypes:");
                        foreach (var f in unusedFragmentDefinitions)
                        {
                            Console.WriteLine("  - " + f);
                        }
                    }

                    // List out all interactions that need to be handled
                    var allUniqueInteractionTypes = s_fragments.Values
                        .SelectMany(f => f.Interactions)
                        .Select(i => i.Type)
                        .Distinct()
                        .OrderBy(x => x.ToString())
                        .ToList();
                    Console.WriteLine("Interaction types that can come up:");
                    foreach (var i in allUniqueInteractionTypes)
                    {
                        Console.WriteLine("  - " + i);
                    }

                }
            }
        }
        #endregion
        #region Helpers

        public class InteractionInfo
        {
            [JsonProperty]
            public InteractionType Type { get; private set; }

            [JsonProperty]
            public TokenType? TokenHint { get; private set; }
        }



        #endregion
    }
}
