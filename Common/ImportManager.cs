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

		public IInteraction ImportEvent()
        {
			string importType = "Event";
			string json = Load(importType);
			var settings = new JsonSerializerSettings
			{
				Converters = new List<JsonConverter> { new InteractionConverter() },
				DefaultValueHandling = DefaultValueHandling.Populate,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.DeserializeObject<IInteraction>(json, settings);
		}

		public Translation ImportTranslation()
		{
			string importType = "Translation";
			return null;
		}

	}
}
