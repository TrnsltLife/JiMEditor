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
    // TODO: some locations are considered to be only accessible in certain core sets, we need to define those separately!

    /// <summary>
    /// Tells what kind of story we are writing.
    /// Maps out a story archetype that exposes entension points that can be fulfilled by Story Templates.
    /// Story is divided in to Start, Middle and End phases that repserenst different parts of the voyage.
    /// </summary>
    class StoryArchetype
    {
        #region Properties

        /// <summary>
        /// Main archetype of the story
        /// </summary>
        [JsonProperty]
        public Type Archetype { get; private set; }

        /// <summary>
        /// Premise for the story where all begings
        /// </summary>
        [JsonProperty]
        public StoryPhase Start { get; private set; }

        /// <summary>
        /// Building up the story towards the End
        /// </summary>
        [JsonProperty]
        public StoryPhase Middle { get; private set; }

        /// <summary>
        /// Resolution and climax of the story
        /// </summary>
        [JsonProperty]
        public StoryPhase End { get; private set; }

        /// <summary>
        /// All possible story fragments as defined in story-archetypes.json
        /// </summary>
        public static IEnumerable<string> AllPossibleStoryFragments { get; private set; }

        /// <summary>
        /// All possible story locations as defined in story-archetypes.json
        /// </summary>
        public static IEnumerable<string> AllPossibleStoryLocations { get; private set; }

        #endregion
        #region Construction and fetching
        private static Dictionary<Type, StoryArchetype> s_archetypes;

        public static StoryArchetype GetArchetype(Type archetype)
        {
            return s_archetypes[archetype];
        }

        public static StoryArchetype GetRandomArchetype(Random r)
        {
            // Randomize archetype
            Array values = Enum.GetValues(typeof(Type));
            var archetype = (Type)values.GetValue(r.Next(values.Length));

            // Get actual archetype
            return GetArchetype(archetype);
        }

        private StoryArchetype() { /* Prevent construction from outside */ }

        static StoryArchetype()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.Contains(".story-archetypes.json"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var list = JObject.Parse(json);
                    s_archetypes = list.SelectToken("archetypes")
                        .ToObject<List<StoryArchetype>>()
                        .ToDictionary(x => x.Archetype, x => x);
                    Console.WriteLine("Story Archetypes Loaded: " + s_archetypes.Count);

                    // Also gather all story fragments to a list from the archetype list
                    AllPossibleStoryFragments = s_archetypes.Values
                        .SelectMany(a => a.Start.MustHaveOneOf
                             .Union(a.Start.CanHaveSomeOf)
                             .Union(a.Middle.MustHaveOneOf)
                             .Union(a.Middle.CanHaveSomeOf)
                             .Union(a.End.MustHaveOneOf)
                             .Union(a.End.CanHaveSomeOf))
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    Console.WriteLine("Story Fragments Identified:");
                    foreach (var f in AllPossibleStoryFragments)
                    {
                        Console.WriteLine("  - " + f);
                    }

                    // Also gather all story locations to a list from the archetype list
                    AllPossibleStoryLocations = s_archetypes.Values
                        .SelectMany(a => a.Start.TakesPlaceInOneOf
                             .Union(a.Middle.TakesPlaceInOneOf)
                             .Union(a.End.TakesPlaceInOneOf))
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    Console.WriteLine("Story Locations Identified:");
                    foreach (var f in AllPossibleStoryLocations)
                    {
                        Console.WriteLine("  - " + f);
                    }
                }
            }
        }
        #endregion
        #region Helpers
        /// <summary>
        /// The actual archetype behind the story, from e.g. https://www.nownovel.com/blog/types-of-stories-archetypes/
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// In the stories, this is where the hero must destroy the monster to restore balance to the world. 
            /// </summary>
            OvercomingTheMonster,

            /// <summary>
            /// In the stories, this is where a modest and moral but downtrodden character achieves a happy ending when their natural talents are displayed to the world at large.
            /// </summary>
            //RagsToRiches,

            /// <summary>
            /// The hero, often accompanied by sidekicks, travels in search of a priceless treasure and must defeat evil and overcome powerful odds, and ends when he gets both the treasure and the girl. 
            /// </summary>
            //TheQuest,

            /// <summary>
            /// Stories of normal protagonists who are suddenly thrust into strange and alien worlds and must make their way back to normal life once more.
            /// </summary>
            //VoyageAndReturn,

            /// <summary>
            /// Not in the “Haha” that’s funny kind of way, but more in the Shakespeare kind of way. The plot of a comedy involves some kind of confusion that must be resolved before the hero and heroine can be united in love.
            /// </summary>
            //Comedy,

            /// <summary>
            /// As a rule, the consequences of human overreaching and egotism. Julius Caesar, Romeo and Juliet, Hamlet etc... Stories from this category are usually very self evident.
            /// </summary>
            //Tragedy,

            /// <summary>
            /// This story archetype almost always has a threatening shadow that seems nearly victorious until a sequence of fortuitous (or even miraculous) events lead to redemption and rebirth, and the restoration of a happier world.
            /// </summary>
            //Rebirth
        }

        /// <summary>
        /// Start, Middle or End of the Story and the contents that belong to it
        /// </summary>
        public class StoryPhase
        {
            [JsonProperty]
            public string Comment { get; private set; }

            /// <summary>
            /// One of the story fragments defined in story-archetypes.json. This one contains the main story elements that NEED to be in the story.
            /// </summary>
            [JsonProperty]
            public List<string> MustHaveOneOf { get; private set; }

            /// <summary>
            /// One of the story fragments defined in story-archetypes.json. This one contains the side story elements that COULD to be in the story.
            /// </summary>
            [JsonProperty]
            public List<string> CanHaveSomeOf { get; private set; }

            /// <summary>
            /// One of the StoryLocations defined in story-locations.json OR STARTING_LOCATION to the one that the story begun 
            /// </summary>
            [JsonProperty]
            public List<string> TakesPlaceInOneOf { get; private set; }
        }

        #endregion
    }
}
