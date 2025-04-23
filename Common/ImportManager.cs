using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace JiME
{
    public class ImportManager
    {
		public string fileName { get; set; }

		private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			DefaultValueHandling = DefaultValueHandling.Populate,
			//NullValueHandling = NullValueHandling.Ignore,
			MissingMemberHandling = MissingMemberHandling.Ignore
		};


		public ImportManager()
        {

        }

		/// <summary>
		/// Save a blog of data to be able to import it into a different scenario. e.g. an Event, a Monster Activation, a Monster Bonus.
		/// Or save a translation for a langauge so a collaborator can work on it and send it back for later import.
		/// 
		/// importType is a type name like "Event" or "Monster" or "Translation".
		/// </summary>
		public string Load(string importType)
		{
			string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Your Journey", "Exports", importType);

			if (!Directory.Exists(basePath))
			{
				basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Your Journey", "Exports");
				if (!Directory.Exists(basePath))
				{
					basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Your Journey");
					if (!Directory.Exists(basePath))
					{
						var di = Directory.CreateDirectory(basePath);
						if (di == null)
						{
							MessageBox.Show("Could not create the " + importType + " export folder.\r\nTried to create: " + basePath, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error);
							return null;
						}
					}
				}
			}

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.DefaultExt = ".jime-" + importType.ToLower();
			openFileDialog.Title = "Export " + importType + " As";
			openFileDialog.Filter = importType + " File (*.jime-" + importType.ToLower() + ")|*.jime-" + importType.ToLower();
			openFileDialog.InitialDirectory = basePath;
			if (openFileDialog.ShowDialog() == true)
				fileName = openFileDialog.FileName;
			else
			{
				return null;
			}

			string json = "";
			try
			{
				using (StreamReader sr = new StreamReader(Path.Combine(basePath, fileName)))
				{
					json = sr.ReadToEnd();
				}

				return json;
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not open the " + importType + " file.\r\n\r\nException:\r\n" + e.Message, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		public MonsterModifier ImportMonsterModifier(Scenario scenario)
		{
			string importType = "Bonus";
			string json = Load(importType);
			if (json == null) { return null; }
			var settings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Populate,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};

			MonsterModifierExportPackage package = JsonConvert.DeserializeObject<MonsterModifierExportPackage>(json, settings);

			//Make sure the dataName we're importing is unique. Add (2), (3), etc. at the end if necessary.
			Dictionary<string, string> dataNameMap = new Dictionary<string, string>();
			foreach (var modifier in scenario.monsterModifierObserver)
			{
				dataNameMap.Add(modifier.dataName, modifier.dataName);
			}
			int copyIndex = 2;
			string baseDataName = package.monsterModifier.dataName;
			while (dataNameMap.ContainsKey(package.monsterModifier.dataName))
			{
				package.monsterModifier.dataName = baseDataName + "(" + copyIndex + ")";
				copyIndex++;
			}

			//Update the translations for this item based on what is saved in the file
			ImportTranslationSubset(scenario, package.translations);

			return package.monsterModifier;
		}

		public MonsterActivations ImportMonsterActivations(Scenario scenario)
        {
			string importType = "Enemy";
			string json = Load(importType);
			if (json == null) { return null; }
			var settings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Populate,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};

			MonsterActivationExportPackage package = JsonConvert.DeserializeObject<MonsterActivationExportPackage>(json, settings);

			//Make sure the dataName we're importing is unique. Add (2), (3), etc. at the end if necessary.
			Dictionary<string, string> dataNameMap = new Dictionary<string, string>();
			foreach (var activations in scenario.activationsObserver)
			{
				dataNameMap.Add(activations.dataName, activations.dataName);
			}
			int copyIndex = 2;
			string baseDataName = package.monsterActivations.dataName;
			while (dataNameMap.ContainsKey(package.monsterActivations.dataName))
			{
				package.monsterActivations.dataName = baseDataName + "(" + copyIndex + ")";
				copyIndex++;
			}

			//Update the translations for this item based on what is saved in the file
			ImportTranslationSubset(scenario, package.translations);

			return package.monsterActivations;
		}

		public IInteraction ImportEvent(Scenario scenario)
        {
			string importType = "Event";
			string json = Load(importType);
			if(json == null) { return null; }
			var settings = new JsonSerializerSettings
			{
				Converters = new List<JsonConverter> { new InteractionConverter() },
				DefaultValueHandling = DefaultValueHandling.Populate,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};

			InteractionExportPackage package = JsonConvert.DeserializeObject<InteractionExportPackage>(json, settings);

			if(package != null && package.interaction != null && package.interaction.interactionType == InteractionType.Threat)
            {
				//If it's a ThreatInteraction, try to match the activationsId based on the name, and the monsterModifiers based on the names
				MatchExtraInfoForThreatInteraction(scenario, package);
			}

			//Update the translations for this item based on what is saved in the file
			ImportTranslationSubset(scenario, package.translations);

			return package.interaction;
		}

		void MatchExtraInfoForThreatInteraction(Scenario scenario, InteractionExportPackage eventPackage)
		{
			ThreatInteraction threat = (ThreatInteraction)eventPackage.interaction;
			//Get a list of all the current activation and modifiers in use in this event
			//Set them to activation/modifier of the same name present in the current scenario, or set them to a default value
			foreach (Monster monster in threat.monsterCollection)
			{
				//Try to match the monster activation name from the imported monster activation with one of the monster activation names in the current scenario
				int activationId = monster.activationsId;
				MonsterActivationInfo activationInfo = eventPackage.activationsReference.FirstOrDefault(it => it.id == activationId);
				MonsterActivations matchingActivation = scenario.activationsObserver.FirstOrDefault(it => it.dataName == activationInfo.name);
				if(matchingActivation != null)
                {
					monster.activationsId = matchingActivation.id;
                }
                else
                {
					monster.activationsId = monster.id; //Set to the default monster figure type like Ruffian, Goblin Scout, etc.
                }


				//Try to match the monster modifier name from the imported monster modifier with one of the monster modifier names in the current scenario
				List<MonsterModifier> removeList = new List<MonsterModifier>();
				foreach (MonsterModifier mod in monster.modifierList)
				{
					MonsterModifierInfo modifierInfo = eventPackage.modifiersReference.FirstOrDefault(it => it.id == mod.id);
					MonsterModifier matchingModifier = scenario.monsterModifierObserver.FirstOrDefault(it => it.dataName == modifierInfo.name);
					if(matchingModifier != null)
                    {
						mod.id = matchingModifier.id;
						mod.name = matchingModifier.name;
						mod.dataName = matchingModifier.dataName;
						mod.CopyData(matchingModifier);
                    }
					else
                    {
						removeList.Add(mod);
					}
				}
				//Remove MonsterModifiers that don't exist in this scenario
				foreach (MonsterModifier modifier in removeList)
				{
					monster.modifierList.Remove(modifier);
				}
			}
		}

		public void ImportTranslation(ObservableCollection<Translation> translations)
		{
			string importType = "Translation";
			string json = Load(importType);
			if (json == null) { return; }

			var settings = new JsonSerializerSettings
			{
				Converters = new List<JsonConverter> { /*new InteractionConverter()*/ },
				DefaultValueHandling = DefaultValueHandling.Populate,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};

			TranslationForExport translationForImport = JsonConvert.DeserializeObject<TranslationForExport>(json, settings);

			//Find a matching translation object that already exists...
			string langCode = translationForImport.dataName;
			string langName = translationForImport.langName;
			Translation translationForUpdate = translations.FirstOrDefault(it => it.dataName == langCode && it.langName == langName);
			if(translationForUpdate == null)
            {
				translationForUpdate = translations.FirstOrDefault(it => it.dataName == langCode);
			}
			//Otherwise create a new one and add it to the list of translations
			if(translationForUpdate == null)
            {
				translationForUpdate = new Translation(langCode);
				translationForUpdate.langName = langName;
				translations.Add(translationForUpdate);
            }

			//Update the translation object from the import file
			Dictionary<string, string> importKeyValuePairs = new Dictionary<string, string>();
			foreach(TranslationItemForExport item in translationForImport.translationItems)
            {
				try
				{
					importKeyValuePairs.Add(item.key, item.text);
				} 
				catch(Exception e) 
				{
					Debug.Log("error importing translation text for language " + langCode + " " + langName + " key: " + item.key + " text: " + item.text); 
				}
            }
			foreach(TranslationItem item in translationForUpdate.translationItems)
            {
				string key = item.key;
				if(importKeyValuePairs.ContainsKey(key))
                {
					if(item.text != importKeyValuePairs[key])
                    {
						item.text = importKeyValuePairs[key];
						item.translationOK = true;
                    }
                }
            }
		}


		public void ImportTranslationSubset(Scenario scenario, List<Translation> importedTranslations)
        {
			foreach(Translation importedTranslation in importedTranslations)
            {
				//First make sure all the languages in the importedTranslations exist in the scenario's translation. If not, create them.
				Translation translation = scenario.translationObserver.FirstOrDefault(it => it.dataName == importedTranslation.dataName);
				if(translation == null)
                {
					translation = new Translation(importedTranslation.dataName);
					translation.langName = importedTranslation.langName;
					scenario.translationObserver.Add(translation);
                }

				//Add all the translation item from the import to the scenario's translation
				foreach(TranslationItem importedItem in importedTranslation.translationItems)
                {
					TranslationItem item = translation.translationItems.FirstOrDefault(it => it.key == importedItem.key);
					if(item == null)
                    {
						item = importedItem.Clone();
						translation.translationItems.Add(item);
                    }
					else
                    {
						item.text = importedItem.text;
						item.translationOK = false; //It might have been OK before but everything imported should be reviewed
                    }
                }
			}
		}
	}
}
