using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace JiME
{
    public class ExportManager
    {
		public string fileName { get; set; }

		public ExportManager()
        {

        }

		/// <summary>
		/// Save a blog of data to be able to import it into a different scenario. e.g. an Event, a Monster Activation, a Monster Bonus.
		/// Or save a translation for a langauge so a collaborator can work on it and send it back for later import.
		/// 
		/// exportType is a type name like "Event" or "Monster" or "Translation". It will be created in a subfolder of "Your Journey/Exports/"
		/// jsonOutput the Json serialized form of the object to be exported
		/// </summary>
		public bool Save(string exportType, string jsonOutput, string suggestedFilename)
		{
			string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Your Journey", "Exports", exportType);

			if (!Directory.Exists(basePath))
			{
				var di = Directory.CreateDirectory(basePath);
				if (di == null)
				{
					MessageBox.Show("Could not create the " + exportType + " export folder.\r\nTried to create: " + basePath, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = ".jime-" + exportType.ToLower();
			saveFileDialog.Title = "Export " + exportType + " As";
			saveFileDialog.Filter = exportType + " File (*.jime-" + exportType.ToLower() + ")|*.jime-" + exportType.ToLower();
			saveFileDialog.InitialDirectory = basePath;
			saveFileDialog.FileName = suggestedFilename;
			if (saveFileDialog.ShowDialog() == true)
				fileName = saveFileDialog.FileName;
			else
				return false;

			//just use the filename, not the whole path
			FileInfo fi = new FileInfo(fileName);
			fileName = fi.Name;

			string output = JsonConvert.SerializeObject(this, Formatting.Indented);
			string outpath = Path.Combine(basePath, fileName);
			try
			{
				using (var stream = File.CreateText(outpath))
				{
					stream.Write(jsonOutput);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not save the " + exportType + " file.\r\n\r\nException:\r\n" + e.Message, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			return true;
		}

		public bool Export(string exportType, object objectToExport)
        {
			Debug.Log("Export " + exportType);
			string jsonOutput = JsonConvert.SerializeObject(objectToExport, Formatting.Indented);
			Debug.Log(jsonOutput);
			return Save(exportType, jsonOutput, (objectToExport as ICommonData).dataName);
		}

		public bool ExportMonsterModifier(Scenario scenario, MonsterModifier modifierToExport)
        {
			string exportType = "Bonus";
			if (modifierToExport == null) { return false; }
			MonsterModifierExportPackage modifierPackage = new MonsterModifierExportPackage();
			modifierPackage.monsterModifier = modifierToExport;

			//Add translation info to modifierPackage.translations
			List<TranslationItem> defaultTranslation = scenario.CollectModifierTranslationItems(modifierToExport);
			List<Translation> translations = CollectTranslationListForExport(scenario, defaultTranslation);
			modifierPackage.translations = translations;

			string jsonOutput = JsonConvert.SerializeObject(modifierPackage, Formatting.Indented);
			return Save(exportType, jsonOutput, (modifierToExport as ICommonData).dataName);
		}

		public bool ExportMonsterActivation(Scenario scenario, MonsterActivations activationToExport)
		{
			string exportType = "Enemy";
			if (activationToExport == null) { return false; }
			MonsterActivationExportPackage activationPackage = new MonsterActivationExportPackage();
			activationPackage.monsterActivations = activationToExport;

			//Add translation info to modifierPackage.translations
			List<TranslationItem> defaultTranslation = scenario.CollectActivationTranslationItems(activationToExport);
			List<Translation> translations = CollectTranslationListForExport(scenario, defaultTranslation);
			activationPackage.translations = translations;

			string jsonOutput = JsonConvert.SerializeObject(activationPackage, Formatting.Indented);
			return Save(exportType, jsonOutput, (activationToExport as ICommonData).dataName);
		}

		public bool ExportEvent(Scenario scenario, InteractionBase eventToExport)
		{
			string exportType = "Event";
			if(eventToExport == null || !(eventToExport is InteractionBase)){ return false; }
			InteractionExportPackage eventPackage = new InteractionExportPackage();
			eventPackage.interaction = (InteractionBase)eventToExport;

			//If it's a ThreatEvent, add info on the current Monster Activations and Monster Bonuses, to help match up or alert if those don't exist when importing
			if(eventToExport is ThreatInteraction)
            {
				AddExtraInfoForThreatInteraction(scenario, eventPackage, eventToExport);			
            }

			//Add translation info to eventPackage.translations
			List<TranslationItem> defaultTranslation = scenario.CollectInteractionTranslationItems(eventToExport);
			List<Translation> translations = CollectTranslationListForExport(scenario, defaultTranslation);
			eventPackage.translations = translations;

			string jsonOutput = JsonConvert.SerializeObject(eventPackage, Formatting.Indented);
			return Save(exportType, jsonOutput, (eventToExport as ICommonData).dataName);
		}

		void AddExtraInfoForThreatInteraction(Scenario scenario, InteractionExportPackage eventPackage, object objectToExport)
        {
			eventPackage.modifiersReference = new List<MonsterModifierInfo>();
			eventPackage.activationsReference = new List<MonsterActivationInfo>();

			ThreatInteraction threat = (ThreatInteraction)objectToExport;
			List<int> activations = new List<int>();
			List<int> modifiers = new List<int>();
			//Get a list of all the current activation and modifiers in use in this event
			foreach (Monster monster in threat.monsterCollection)
			{
				int activationId = monster.activationsId;
				if (!activations.Contains(activationId))
				{
					activations.Add(activationId);
					string name = scenario.activationsObserver.First(it => it.id == activationId).dataName;
					eventPackage.activationsReference.Add(new MonsterActivationInfo(activationId, name));
					Debug.Log("Add activation " + activationId + " " + name);
				}

				foreach (MonsterModifier mod in monster.modifierList)
				{
					if (!modifiers.Contains(mod.id))
					{
						modifiers.Add(mod.id);
						string name = scenario.monsterModifierObserver.First(it => it.id == mod.id).name;
						eventPackage.modifiersReference.Add(new MonsterModifierInfo(mod.id, name));
						Debug.Log("Add modifier " + mod.id + " " + name);
					}
				}
			}
		}

		public bool ExportTranslation(string scenarioName, TranslationForExport objectToExport)
		{
			string exportType = "Translation";
			Debug.Log("Export " + exportType);
			string jsonOutput = JsonConvert.SerializeObject(objectToExport, Formatting.Indented);
			Debug.Log(jsonOutput);
			return Save(exportType, jsonOutput, scenarioName + "-" + objectToExport.dataName + "-" + objectToExport.langName);
		}

		public List<Translation> CollectTranslationListForExport(Scenario scenario, List<TranslationItem> defaultTranslation)
        {
			List<Translation> translationList = new List<Translation>();
			foreach(Translation translationInitialState in scenario.translationObserver)
            {
				Translation translationSubset = new Translation(translationInitialState.dataName);
				translationSubset.langName = translationInitialState.langName;
				translationList.Add(translationSubset);
				CollectTranslationSubset(defaultTranslation, translationInitialState, translationSubset);
            }

			return translationList;
        }

		public void CollectTranslationSubset(List<TranslationItem> defaultTranslation, Translation translationInitialState, Translation translationSubset)
		{
			Dictionary<string, TranslationItem>  translationDict = new Dictionary<string, TranslationItem>(); //hold the keys from the language translation for easy lookup as we add in keys from the default translation that are missing in the language translation

			//First copy all the TranslationItems from the language translation into the translation dictionary
			foreach (var item in translationInitialState.translationItems)
			{
				TranslationItem clonedItem = item.Clone();
				//translation.translationItems.Add(clonedItem);
				translationDict.Add(item.key, clonedItem);
			}

			//Now insert items from defaultTranslation that have a match in translationDict into translationSubset
			foreach (var item in defaultTranslation)
            {
				if(translationDict.ContainsKey(item.key))
                {
					translationSubset.translationItems.Add(translationDict[item.key]);
				}
            }
		}
	}
}
