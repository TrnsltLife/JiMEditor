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

		public bool ExportTranslation(string scenarioName, TranslationForExport objectToExport)
		{
			string exportType = "Translation";
			Debug.Log("Export " + exportType);
			string jsonOutput = JsonConvert.SerializeObject(objectToExport, Formatting.Indented);
			Debug.Log(jsonOutput);
			return Save(exportType, jsonOutput, scenarioName + "-" + objectToExport.dataName + "-" + objectToExport.langName);
		}

	}
}
