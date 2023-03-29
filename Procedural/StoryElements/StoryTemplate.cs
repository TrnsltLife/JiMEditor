using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JiME.Procedural.GenerationLogic;
using System.Text.RegularExpressions;

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
        public string Name { get; private set; }
        
        [JsonProperty]
        public List<RaceEnum> AntagonistIsOneOf { get; private set; }

        [JsonProperty]
        public List<MonsterType> AntagonistMonsterIsOneOf { get; private set; }

        [JsonProperty]
        public List<MonsterType> AntagonistHelperMonstersAreSomeOf { get; private set; }

        [JsonProperty]
        public List<RaceEnum> BystandersAreOneOf { get; private set; }

        [JsonProperty]
        public Dictionary<StoryArchetype.Type, List<string>> ScenarioIntroductions { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<ObjectiveFragmentInfo>> ScenarioObjectives { get; private set; }

        // TODO: public bool IsValidForCollections(IEnumerable<Collection>()) { Check if can be used with current collections }
        // TODO: Or "AdjustForCollections()" which would filter out extras

        #endregion
        #region Data getters

        public TemplateContext PrepareTemplateContext(Random r)
        {
            return new TemplateContext()
            {
                SelectedAntagonistRace = AntagonistIsOneOf.GetRandomFromEnumerable(r),
                SelectedBystanderRace = BystandersAreOneOf.GetRandomFromEnumerable(r),
                PersistentTranslations = new Dictionary<string, string>()
            };
        }

        public string GenerateScenarioIntroduction(Random r, StoryArchetype.Type archetype, TemplateContext tokenCtx)
        {
            var text = ScenarioIntroductions[archetype].GetRandomFromEnumerable(r);
            return ProcessTextTemplate(text, r, tokenCtx);
        }

        /// <summary>
        /// Generates reminder text and intro text for an objective
        /// </summary>
        public Tuple<string, string> GenerateObjectiveInformation(Random r, string storyFragment, TemplateContext tokenCtx)
        {
            var objectInfo = ScenarioObjectives[storyFragment].GetRandomFromEnumerable(r);
            return Tuple.Create(
                ProcessTextTemplate(objectInfo.Reminder, r, tokenCtx),
                ProcessTextTemplate(objectInfo.IntroText, r, tokenCtx)
            );
        }

        /// <summary>
        /// Processes any {...} in the text
        /// </summary>
        private string ProcessTextTemplate(string text, Random r, TemplateContext tokenCtx)
        {
            var d = new Dictionary<string, string>(); // <-- keep track of already translated entities and re-use them
            return Regex.Replace(text, "\\{.*?\\}", match =>
            {
                // Check if already translated
                var key = match.Value;
                if (d.ContainsKey(key))
                {
                    return d[key];
                }

                // Translate from the dictionary
                string result;
                var data = key.Trim('{', '}').Split(':');
                var dataDict = s_tokens[data[0]];
                var dataKey = data[1];
                if (dataKey == KEYWORD_BYSTANDER)
                {
                    // Always new random item from bystander race
                    result = dataDict[tokenCtx.SelectedBystanderRace.ToString()].GetRandomFromEnumerable(r);
                }
                else if (dataKey == KEYWORD_ANTAGONIST)
                {
                    // Antagonist data is persistent so we check if from the context
                    var antagonistKey = data[0] + ":" + data[1];
                    if (!tokenCtx.PersistentTranslations.ContainsKey(antagonistKey))
                    {
                        // Antagonist data not selected yet, do it now
                        var antagonistResult = dataDict[tokenCtx.SelectedAntagonistRace.ToString()].GetRandomFromEnumerable(r);
                        tokenCtx.PersistentTranslations.Add(antagonistKey, antagonistResult);
                    }
                    result = tokenCtx.PersistentTranslations[antagonistKey];
                }
                else if (dataDict.ContainsKey(dataKey))
                {
                    result = dataDict[dataKey].GetRandomFromEnumerable(r);
                }
                else
                {
                    // Could not parse
                    result = key;
                }

                // TODO: how to handle preposition (a/an)?

                // Store result and return
                d.Add(key, result);
                return result;
            });
        }

        #endregion
        #region Construction and fetching
        private static Dictionary<string, StoryTemplate> s_templates;

        private static Dictionary<string, Dictionary<string, List<string>>> s_tokens;

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

                    s_tokens = list.SelectToken("tokens").ToObject<Dictionary<string, Dictionary<string, List<string>>>>();
                }
            }
        }
        #endregion
        #region Helpers

        /// <summary>
        /// Race identifier used in story generation
        /// </summary>
        public enum RaceEnum
        {
            HUMAN,
            ELF,
            DWARF,
            HOBBIT,
            ORC,
            TROLL
        }

        public static PersonType GetPersonType(RaceEnum race)
        {
            switch (race)
            {
                case RaceEnum.DWARF:
                    return PersonType.Dwarf;
                case RaceEnum.ELF:
                    return PersonType.Elf;
                case RaceEnum.HOBBIT:
                    return PersonType.Hobbit;
                // Humans and all the others use human tokens if needed
                default:
                    return PersonType.Human;
            }
        }

        public static string KEYWORD_BYSTANDER = "BYSTANDER";
        public static string KEYWORD_ANTAGONIST = "ANTAGONIST";

        /// <summary>
        /// Storage for certain locked in data for story generation 
        /// </summary>
        public class TemplateContext
        {
            public RaceEnum SelectedBystanderRace { get; set; }

            public RaceEnum SelectedAntagonistRace { get; set; }

            public Dictionary<string, string> PersistentTranslations { get; set; }

            public string GetAntagonistName()
            {
                var key = "names:" + KEYWORD_ANTAGONIST;
                return PersistentTranslations[key]; // TODO: how to make sure this has been generated?
            }
        }

        /// <summary>
        /// Info about 
        /// </summary>
        public class ObjectiveFragmentInfo
        {
            [JsonProperty]
            public string Reminder { get; private set; }

            [JsonProperty]
            public string IntroText { get; private set; }
        }

        #endregion
    }
}
