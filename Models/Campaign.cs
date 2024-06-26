﻿using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class Campaign : INotifyPropertyChanged
	{
		string _campaignName, _campaignVersion, _fileVersion, _coverImage;

		public Guid campaignGUID;
		public string campaignName
		{
			get => _campaignName;
			set
			{
				_campaignName = value;
				PropChanged( "campaignName" );
			}
		}
		public string campaignVersion
		{
			get => _campaignVersion;
			set
			{
				_campaignVersion = value;
				PropChanged("campaignVersion");
			}
		}
		public string fileVersion
		{
			get => _fileVersion;
			set
			{
				_fileVersion = value;
				PropChanged( "fileVersion" );
			}
		}
		public string coverImage
		{
			get => _coverImage;
			set
			{
				_coverImage = value;
				PropChanged("coverImage");
			}
		}

		public string storyText { get; set; }
		public string description { get; set; }

		public bool startWithTrinkets { get; set; }
		public bool startWithMounts { get; set; }

		public ObservableCollection<CampaignItem> scenarioCollection { get; set; }
		public ObservableCollection<Trigger> triggerCollection { get; set; }
        [JsonIgnore]
		public ObservableCollection<Collection> collectionCollection { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public Campaign()
		{
			campaignGUID = Guid.NewGuid();
			scenarioCollection = new ObservableCollection<CampaignItem>();
			triggerCollection = new ObservableCollection<Trigger>();
			collectionCollection = new ObservableCollection<Collection>();
			campaignName = "";
			storyText = "";
			description = "";
			fileVersion = Utils.formatVersion;
			coverImage = null;
			startWithTrinkets = false;
			startWithMounts = false;
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
