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
    // TODO: Some locations in story-locations.json could be split up, e.g. dungeons has multiple different styles that could be their own sets
    //       perhaps naming them with numbet suffix and always just choose one the location sun-types?

    /// <summary>
    /// Gives more information and a possible tile list for certain location e.g. "Woodlands" or "Cave"
    /// </summary>
    class StoryLocation
    {
        #region Constants

        /// <summary>
        /// Default effect to add to all exploration notes from tiles.
        /// </summary>
        public static readonly string DEFAULT_EXPLORATION_EFFECT = "Discard the exploration token. Gain 1 inspiration.";

        #endregion
        #region Properties

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty ("KnownTiles")]
        private List<TileInfo> KnownTilesInternal { get; set; }

        [JsonIgnore]
        public IDictionary<string, TileInfo> KnownTiles => KnownTilesInternal.ToDictionary(x => x.IdAndSide, x => x);
 
        #endregion
        #region Construction and fetching
        private static Dictionary<string, StoryLocation> s_locations;

        public static StoryLocation GetLocation(string template)
        {
            return s_locations[template];
        }

        public static StoryLocation GetRandomLocation(Random r)
        {
            // Randomize archetype
            var possibleKeys = s_locations.Keys.ToList();
            var templateKey = possibleKeys[r.Next(possibleKeys.Count)];

            // Get actual archetype
            return GetLocation(templateKey);
        }

        public static IEnumerable<string> GetAllKnownLocationNames()
        {
            return s_locations.Keys.ToList();
        }

        private StoryLocation() { /* Prevent construction from outside */ }

        static StoryLocation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.Contains(".story-locations.json"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var list = JObject.Parse(json);
                    s_locations = list.SelectToken("locations")
                        .ToObject<List<StoryLocation>>()
                        .ToDictionary(x => x.Name, x => x);

                    // Check what we have or have not from the archetypes list
                    var missingLocationDefinitions = StoryArchetype.AllPossibleStoryLocations.Except(s_locations.Keys).ToList();
                    var unusedLocationDefinitions = s_locations.Keys.Except(StoryArchetype.AllPossibleStoryLocations).ToList();

                    if (missingLocationDefinitions.Count > 0)
                    {
                        Console.WriteLine("Story Locations used in StoryArchetypes but NOT DEFINED:");
                        foreach (var f in missingLocationDefinitions)
                        {
                            Console.WriteLine("  - " + f);
                        }
                    }

                    if (unusedLocationDefinitions.Count > 0)
                    {
                        Console.WriteLine("Story Locations defined here but NOT USED in StoryArchetypes:");
                        foreach (var f in unusedLocationDefinitions)
                        {
                            Console.WriteLine("  - " + f);
                        }
                    }

                }
            }
        }
        #endregion
        #region Helpers

        public class TileInfo
        {

            [JsonProperty]
            public string IdAndSide { get; private set; }

            public int IdNumber => int.Parse(IdAndSide.Substring(0, 3));
            public string TileSide => IdAndSide.Last().ToString();

            /// <summary>
            /// Introductionary text when the tile is explored
            /// </summary>
            [JsonProperty]
            public List<string> ExplorationTexts { get; private set; }
        }

        #endregion
    }
}
