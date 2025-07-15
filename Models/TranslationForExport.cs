using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class TranslationForExport
	{

		string _dataName; //contains the language code
		string _langName;
		List<TranslationItemForExport> _translationItems = new List<TranslationItemForExport>();

		public Guid GUID { get; set; }

		public string dataName
		{
			get => _dataName;
			set
			{
				if (value != _dataName)
				{
					_dataName = value;
				}
			}
		}

		public string langName
		{
			get => _langName;
			set
			{
				if (value != _langName)
				{
					_langName = value;
				}
			}
		}

		public List<TranslationItemForExport> translationItems
		{
			get => _translationItems;
			set
			{
				if (value != _translationItems)
				{
					_translationItems = value;
				}
			}
		}


		public TranslationForExport()
		{
			GUID = Guid.NewGuid();
		}

		public TranslationForExport(string dataName, string langName, List<TranslationItemForExport> translationItems)
		{
			GUID = Guid.NewGuid();
			this.dataName = dataName;
			this.langName = langName;
			this.translationItems = translationItems;
		}

	}
}
