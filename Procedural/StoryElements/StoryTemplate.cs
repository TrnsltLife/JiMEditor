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
    // TODO: consider moving story templates each to their own file
    // TODO: consider how templates could be more generic? could they actually serve more than one archetype?

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

        [JsonProperty]  // TODO: We might need to make each template match only single archetype to keep defining the template texts reasonable. Let's see
        public Dictionary<StoryArchetype.Type, List<string>> ScenarioIntroductions { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<ObjectiveInfo>> ScenarioObjectives { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<DialogInteractionInfo>> DialogInteractions { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<StatTestInteractionInfo>> StatTestInteractions { get; private set; }

        [JsonProperty]
        public List<ThreatInteractionInfo> ThreatInteractions { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<string>> Resolutions { get; private set; }

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
        public ObjectiveInfo GenerateObjectiveInformation(Random r, string storyFragment, TemplateContext tokenCtx)
        {
            var objectInfo = ScenarioObjectives[storyFragment].GetRandomFromEnumerable(r);
            return new ObjectiveInfo()
            {
                Reminder = ProcessTextTemplate(objectInfo.Reminder, r, tokenCtx),
                IntroText = ProcessTextTemplate(objectInfo.IntroText, r, tokenCtx)
            };
        }

        public DialogInteractionInfo GenerateDialogInteractionInfo(Random r, string storyFragment, TemplateContext tokenCtx)
        {
            var info = DialogInteractions[storyFragment].GetRandomFromEnumerable(r);
            return new DialogInteractionInfo()
            {
                ActionText = ProcessTextTemplate(info.ActionText, r, tokenCtx),
                Choice1Text = ProcessTextTemplate(info.Choice1Text, r, tokenCtx),
                Choice1Triggers = info.Choice1Triggers,
                Result1Text = ProcessTextTemplate(info.Result1Text, r, tokenCtx),
                Choice2Text = ProcessTextTemplate(info.Choice2Text, r, tokenCtx),
                Choice2Triggers = info.Choice2Triggers,
                Result2Text = ProcessTextTemplate(info.Result2Text, r, tokenCtx),
                Choice3Text = ProcessTextTemplate(info.Choice3Text, r, tokenCtx),
                Choice3Triggers = info.Choice3Triggers,
                Result3Text = ProcessTextTemplate(info.Result3Text, r, tokenCtx),
                PersistentText = ProcessTextTemplate(info.PersistentText, r, tokenCtx),
            };
        }

        public StatTestInteractionInfo GenerateStatTestInteractionInfo(Random r, string storyFragment, TemplateContext tokenCtx)
        {
            var info = StatTestInteractions[storyFragment].GetRandomFromEnumerable(r);
            return new StatTestInteractionInfo()
            {
                StatTestType = info.StatTestType,
                StatHint = info.StatHint,
                AltStatHint = info.AltStatHint,
                SuccessValue = info.SuccessValue,
                TokenText = ProcessTextTemplate(info.TokenText, r, tokenCtx),
                ActionText = ProcessTextTemplate(info.ActionText, r, tokenCtx),
                SuccessText = ProcessTextTemplate(info.SuccessText, r, tokenCtx),
                ProgressText = ProcessTextTemplate(info.ProgressText, r, tokenCtx),
                FailureText = ProcessTextTemplate(info.FailureText, r, tokenCtx)
            };
        }

        public ThreatInteractionInfo GenerateThreatInteractionInfo(Random r, string storyFragment, TemplateContext tokenCtx)
        {
            var info = ThreatInteractions.GetRandomFromEnumerable(r);
            return new ThreatInteractionInfo()
            {
                AntagonistTokenText = ProcessTextTemplate(info.AntagonistTokenText, r, tokenCtx),
                AntagonistActionText = ProcessTextTemplate(info.AntagonistActionText, r, tokenCtx),
                NormalTokenText = ProcessTextTemplate(info.NormalTokenText, r, tokenCtx),
                NormalActionText = ProcessTextTemplate(info.NormalActionText, r, tokenCtx)
            };
        }

        public string GenerateResolutionText(Random r, string lastStoryFragment, TemplateContext tokenCtx)
        {
            if (Resolutions.ContainsKey(lastStoryFragment))
            {
                var text = Resolutions[lastStoryFragment].GetRandomFromEnumerable(r);
                return ProcessTextTemplate(text, r, tokenCtx);
            }
            else
            {
                return "Template: '" + Name + "' Missing resolution text for " + lastStoryFragment;
            }
        }

        /// <summary>
        /// Processes any {...} in the text
        /// </summary>
        private string ProcessTextTemplate(string text, Random r, TemplateContext tokenCtx)
        {
            if (text == null)
            {
                return null;
            }

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

        public class ObjectiveInfo
        {
            [JsonProperty]
            public string Reminder { get; set; }

            [JsonProperty]
            public string IntroText { get; set; }
        }

        public class DialogInteractionInfo
        {
            [JsonProperty]
            public string ActionText { get; set; }

            [JsonProperty]
            public string Choice1Text { get; set; }

            [JsonProperty]
            public bool Choice1Triggers { get; set; }

            [JsonProperty]
            public string Result1Text { get; set; }

            [JsonProperty]
            public string Choice2Text { get; set; }

            [JsonProperty]
            public bool Choice2Triggers { get; set; }

            [JsonProperty]
            public string Result2Text { get; set; }

            [JsonProperty]
            public string Choice3Text { get; set; }

            [JsonProperty]
            public bool Choice3Triggers { get; set; }

            [JsonProperty]
            public string Result3Text { get; set; }

            [JsonProperty]
            public string PersistentText { get; set; }
        }

        public class StatTestInteractionInfo
        {
            [JsonProperty]
            public string TokenText { get; set; }

            [JsonProperty]
            public string ActionText { get; set; }

            [JsonProperty]
            public int SuccessValue { get; set; }

            [JsonProperty]
            public string SuccessText { get; set; }

            [JsonProperty]
            public string ProgressText { get; set; }

            [JsonProperty]
            public string FailureText { get; set; }

            /// <summary>
            /// Used with StatTest
            /// </summary>
            [JsonProperty]
            public Ability StatHint { get; set; }

            /// <summary>
            /// Used with StatTest (OPTIONAL)
            /// </summary>
            [JsonProperty]
            public Ability? AltStatHint { get; set; }

            /// <summary>
            /// Used with StatTest
            /// </summary>
            [JsonProperty]
            public TypeEnum StatTestType { get; set; }

            public enum TypeEnum
            {
                /// <summary>
                /// Test can only be tried once. If it fails a fail is triggered.
                /// </summary>
                OneTry,

                /// <summary>
                /// Test can be tried multiple times and cannot fail.
                /// </summary>
                Retryable,

                /// <summary>
                /// Test can be tried multiple times and successes are added until goal is reached.
                /// </summary>
                Cumulative
            }
        }

        public class ThreatInteractionInfo
        {
            [JsonProperty]
            public string AntagonistTokenText { get; set; }

            [JsonProperty]
            public string AntagonistActionText { get; set; }

            [JsonProperty]
            public string NormalTokenText { get; set; }

            [JsonProperty]
            public string NormalActionText { get; set; }
        }

        #endregion
    }
}
