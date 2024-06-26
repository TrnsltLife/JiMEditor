﻿using System;
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
	public class Translation : INotifyPropertyChanged
	{

		string _dataName; //contains the language code
		string _langName;
		//TODO Change this to a Dictionary instead?
		ObservableCollection<TranslationItem> _translationItems = new ObservableCollection<TranslationItem>();

		public Guid GUID { get; set; }

		public string dataName
		{
			get => _dataName;
			set
			{
				if (value != _dataName)
				{
					_dataName = value;
					NotifyPropertyChanged("dataName");
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
					NotifyPropertyChanged("langName");
				}
			}
		}

		public ObservableCollection<TranslationItem> translationItems
		{
			get => _translationItems;
			set
			{
				if (value != _translationItems)
				{
					_translationItems = value;
					NotifyPropertyChanged("translationItems");
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public Translation()
		{
			GUID = Guid.NewGuid();
		}

		public Translation(string dataName)
		{
			GUID = Guid.NewGuid();
			this.dataName = dataName;
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		public Translation Clone(string newLangCode)
        {
			Translation newTranslation = new Translation { dataName = newLangCode, GUID = Guid.NewGuid() };
			newTranslation.translationItems = new ObservableCollection<TranslationItem>();
			for(int i=0; i<translationItems.Count; i++)
            {
				newTranslation.translationItems.Add(translationItems[i].Clone());
            }
			return newTranslation;
        }

		public void UpdateKeysStartingWith(string originalPrefix, string newPrefix)
        {
			//Call this to update a key or set of keys when the dataName (etc.) changes. e.g. when an Event changes names, and the translation needs to change from event.First Name.etc to event.Changed Name.etc
			if (originalPrefix != newPrefix)
			{
				//translationItems.Where(it => it.key.StartsWith(originalPrefix)).ToList().ForEach(it => it.key = Regex.Replace(it.key, "^" + Regex.Escape(originalPrefix), newPrefix));
				foreach(var item in translationItems)
                {
					if(item.key.StartsWith(originalPrefix))
                    {
						item.key = Regex.Replace(item.key, "^" + Regex.Escape(originalPrefix), newPrefix);
                    }
                }
			}
        }

		public void DecertifyKey(string key)
        {
			//Mark the translation item as needing retranslation/review
			translationItems.Where(it => it.key == key).ToList().ForEach(it => it.translationOK = false);
        }

		public void SetNeedsTranslation(string key)
        {
			translationItems.Where(it => it.key == key).ToList().ForEach(it => it.translationOK = true);
        }
	}
}
