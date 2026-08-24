using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class Campaign : INotifyPropertyChanged
	{
		string _campaignName, _campaignVersion, _fileVersion, _coverImage;

		public Guid campaignGUID;
		bool _isDirty;
		int _dirtySuppress;

		[JsonIgnore]
		public bool isDirty
		{
			get => _isDirty;
			set
			{
				if ( value != _isDirty )
				{
					_isDirty = value;
					PropChanged( "isDirty" );
				}
			}
		}

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

		/// <summary>Mark the campaign as having unsaved edits (no-op if already dirty or inside <see cref="PauseDirtyTracking"/>).</summary>
		public void MarkDirty()
		{
			if ( _dirtySuppress > 0 || isDirty )
				return;
			isDirty = true;
		}

		public void ClearDirty() => isDirty = false;

		/// <summary>Ignore collection/input dirty marks while the campaign is filled programmatically (hydrate).</summary>
		public IDisposable PauseDirtyTracking()
		{
			_dirtySuppress++;
			return new DirtySuppressScope( this );
		}

		void ResumeDirtyTracking()
		{
			if ( _dirtySuppress > 0 )
				_dirtySuppress--;
		}

		sealed class DirtySuppressScope : IDisposable
		{
			Campaign? _campaign;

			public DirtySuppressScope( Campaign campaign ) => _campaign = campaign;

			public void Dispose()
			{
				_campaign?.ResumeDirtyTracking();
				_campaign = null;
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
