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
    /// Gives context and surroundings to the story
    /// Holds information on one single coherent storyline with multiple solutions to each StoryPoint.
    /// Also holds information on base difficulty level of the scenario.
    /// Used in conjunction with StoryArchetype to flesh out a single story
    /// </summary>
    class StoryTemplate
    {
        #region Properties

        [JsonProperty]
        public string Name{ get; private set; }

        // TODO: public bool IsValidForCollections(IEnumerable<Collection>()) { Check if can be used with current collections }
 
        #endregion
        #region Construction and fetching
        private static Dictionary<string, StoryTemplate> s_templates;

        public static StoryTemplate GetTemplate(string template)
        {
            return s_templates[template];
        }

        public static StoryTemplate GetRandomTemplate(Random r)
        {
            // Randomize archetype
            var possibleKeys = s_templates.Keys.ToList();
            var templateKey = possibleKeys[r.Next(possibleKeys.Count)];

            // Get actual archetype
            return GetTemplate(templateKey);
        }

        public static IEnumerable<string> GetAllKnownTemplates()
        {
            return s_templates.Keys.ToList();
        }

        private StoryTemplate() { /* Prevent construction from outside */ }

        static StoryTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.Contains(".story-templates.json"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var list = JObject.Parse(json);
                    s_templates = list.SelectToken("templates")
                        .ToObject<List<StoryTemplate>>()
                        .ToDictionary(x => x.Name, x => x);
                    Console.WriteLine("Story Templates Loaded: " + s_templates.Count);
                }
            }
        }
        #endregion
        #region Helpers

        #endregion
    }
}
