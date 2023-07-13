using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JiME
{
	public abstract class InteractionBase : Translatable, IInteraction, INotifyPropertyChanged
	{
		override public string TranslationKeyName() { return dataName; }
		override public string TranslationKeyPrefix() { return String.Format("event.{1}.{0}.", TranslationKeyName(), translationKeyParents); }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("event.{1}.{0}.tokenText", () => this.isTokenInteraction ? this.tokenInteractionText : ""),
				new TranslationAccessor("event.{1}.{0}.flavorText", () => this.textBookData.pages[0].StartsWith("Default Flavor Text") ? "" : this.textBookData.pages[0]),
				new TranslationAccessor("event.{1}.{0}.eventText", () => this.eventBookData.pages[0].StartsWith("Default Event Text") ? "" : this.eventBookData.pages[0])
			};
			translationAccessors = list;
		}

		string _dataName, _triggerName, _triggerAfterName, _tokenInteractionText;
		bool _isTokenInteraction, _isReusable;
		int _loreReward, _xpReward, _threatReward;
		TokenType _tokenType;
		PersonType _personType;
		TerrainType _terrainType;

		public virtual void CloneInto(InteractionBase interact)
		{
			interact.GUID = Guid.NewGuid();
			interact.dataName = "Copy of " + this.dataName;
			interact.triggerName = this.triggerName;
			interact.triggerAfterName = this.triggerAfterName;
			interact.isTokenInteraction = this.isTokenInteraction;
			interact.loreReward = this.loreReward;
			interact.xpReward = this.xpReward;
			interact.threatReward = this.threatReward;
			interact.tokenType = this.tokenType;
			interact.personType = this.personType;
			interact.terrainType = this.terrainType;
			interact.isEmpty = this.isEmpty;
			interact.textBookData = this.textBookData.Clone();
			interact.eventBookData = this.eventBookData.Clone();
			interact.interactionType = this.interactionType;
			interact.tokenInteractionText = this.tokenInteractionText;
			interact.isReusable = this.isReusable;
		}

		public string dataName
		{
			get { return _dataName; }
			set
			{
				if ( _dataName != value )
				{
					_dataName = value;
					NotifyPropertyChanged( "dataName" );
				}
			}
		}
		public Guid GUID { get; set; }
		public bool isEmpty { get; set; }
		public string triggerName
		{
			get => _triggerName;
			set
			{
				_triggerName = value;
				NotifyPropertyChanged( "triggerName" );
			}
		}
		public string triggerAfterName
		{
			get => _triggerAfterName;
			set
			{
				_triggerAfterName = value;
				NotifyPropertyChanged( "triggerAfterName" );
			}
		}
		public string tokenInteractionText
		{
			get => _tokenInteractionText;
			set
			{
				_tokenInteractionText = value;
				NotifyPropertyChanged("tokenInteractionText");
			}
		}
		public bool isTokenInteraction
		{
			get => _isTokenInteraction;
			set
			{
				_isTokenInteraction = value;
				NotifyPropertyChanged( "isTokenInteraction" );
			}
		}
		public TokenType tokenType
		{
			get => _tokenType;
			set
			{
				_tokenType = value;
				NotifyPropertyChanged( "tokenType" );
			}
		}
		public PersonType personType
		{
			get => _personType;
			set { _personType = value; NotifyPropertyChanged( "personType" ); }
		}

		public TerrainType terrainType
		{
			get => _terrainType;
			set { _terrainType = value; NotifyPropertyChanged("terrainType"); }
		}

		public TextBookData textBookData { get; set; }
		public TextBookData eventBookData { get; set; }
		public int loreReward
		{
			get => _loreReward;
			set
			{
				_loreReward = value;
				NotifyPropertyChanged( "loreReward" );
			}
		}
		public int xpReward
		{
			get => _xpReward;
			set
			{
				_xpReward = value;
				NotifyPropertyChanged( "xpReward" );
			}
		}
		public int threatReward
		{
			get => _threatReward;
			set
			{
				_threatReward = value;
				NotifyPropertyChanged( "threatReward" );
			}
		}
		public bool isReusable
		{
			get => _isReusable;
			set
			{
				_isReusable = value;
				NotifyPropertyChanged("isReusable");
			}
		}


		public InteractionType interactionType { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public InteractionBase( string name )
		{
			dataName = name;
			GUID = Guid.NewGuid();
			isEmpty = false;
			triggerName = "None";
			triggerAfterName = "None";
			isTokenInteraction = false;
			tokenType = TokenType.Search;
			personType = PersonType.None;
			terrainType = TerrainType.None;
			textBookData = new TextBookData();
			textBookData.pages.Add( "Default Flavor Text\n\nUse this text to describe the Event situation and present choices, depending on the type of Event this is." );
			eventBookData = new TextBookData();
			eventBookData.pages.Add( "Default Event Text.\n\nThis text is shown after the Event is triggered. Use it to tell about the actual event that has been triggered Example: Describe an Enemy Threat, present a Test, describe a Decision, etc." );
			loreReward = xpReward = threatReward = 0;
			tokenInteractionText = "";
			isReusable = false;
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		public void RenameTrigger( string oldName, string newName )
		{
			if ( triggerName == oldName )
				triggerName = newName;

			if ( triggerAfterName == oldName )
				triggerAfterName = newName;
		}
	}
}
