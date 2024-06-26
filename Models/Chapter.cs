﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace JiME
{
	/// <summary>
	/// A Chapter contains a batch of Tiles. Each Chapter leads to the next by activating a Trigger
	/// </summary>
	public class Chapter : Translatable, INotifyPropertyChanged, ICommonData
	{

		override public string TranslationKeyName() { return dataName; }
		override public string TranslationKeyPrefix() { return String.Format("chapter.{0}.", TranslationKeyName()); }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("chapter.{0}.exploredText", () => this.noFlavorText ? "" : this.flavorBookData.pages[0])
			};
			translationAccessors = list;
		}

		public void UpgradeChapter()
        {
			if (dataName == "Start" && isEmpty)
			{
				isStart = true;
			}
		}

		//common
		string _dataName;

		public string dataName
		{
			get => _dataName;
			set
			{
				if ( _dataName != value )
				{
					_dataName = value;
					PropChanged( "dataName" );

					if (_dataName == "Start" && isEmpty)
					{
						isStart = true;
					}
				}
			}
		}
		public Guid GUID { get; set; }
		public bool isEmpty { get; set; }
		public string triggerName { get; set; }

		//vars
		bool _noFlavorText, _isRandomTiles, _isPreExplored, _usesRandomGroups, _isDynamic, _isStart;
		string _triggeredBy, _exploreTrigger, _exploredAllTilesTrigger, _randomInteractionGroup, _attachHint;
		int _randomInteractionGroupCount, _attachTileHint;
		DensityPreference _tileDensityPreference;

		public bool noFlavorText
		{
			get => _noFlavorText;
			set
			{
				_noFlavorText = value;
				PropChanged( "noFlavorText" );
			}
		}
		public TextBookData flavorBookData { get; set; }
		public string triggeredBy
		{
			get => _triggeredBy;
			set
			{
				_triggeredBy = value;
				PropChanged( "triggeredBy" );
			}
		}
		public string exploreTrigger
		{
			get => _exploreTrigger;
			set
			{
				_exploreTrigger = value;
				PropChanged( "exploreTrigger" );
			}
		}
		public string exploredAllTilesTrigger
		{
			get => _exploredAllTilesTrigger;
			set
			{
				_exploredAllTilesTrigger = value;
				PropChanged("exploredAllTilesTrigger");
			}
		}
		public bool isRandomTiles
		{
			get => _isRandomTiles;
			set
			{
				_isRandomTiles = value;
				PropChanged( "isRandomTiles" );
			}
		}
		[JsonConverter( typeof( TileConverter ) )]
		public ObservableCollection<ITile> tileObserver { get; set; }
		public string randomInteractionGroup
		{
			get => _randomInteractionGroup;
			set
			{
				_randomInteractionGroup = value;
				PropChanged( "randomInteractionGroup" );
			}
		}
		public int randomInteractionGroupCount
		{
			get => _randomInteractionGroupCount;
			set
			{
				_randomInteractionGroupCount = value;
				PropChanged( "randomInteractionGroupCount" );
			}
		}
		public bool isStart
		{
			get => _isStart;
			set { _isStart = value; PropChanged("isStart"); }
		}
		public bool isPreExplored
		{
			get => _isPreExplored;
			set { _isPreExplored = value; PropChanged( "isPreExplored" ); }
		}
		public bool usesRandomGroups
		{
			get => _usesRandomGroups;
			set
			{
				_usesRandomGroups = value;
				PropChanged( "usesRandomGroups" );
			}
		}
		public bool isDynamic
		{
			get => _isDynamic;
			set
			{
				_isDynamic = value;
				PropChanged( "isDynamic" );
			}
		}
		public string attachHint
		{
			get => _attachHint;
			set
			{
				_attachHint = value;
				PropChanged( "attachHint" );
			}
		}

		public int attachTileHint
		{
			get => _attachTileHint;
			set
			{
				_attachTileHint = value;
				PropChanged("attachTileHint");
			}
		}

		public DensityPreference tileDensityPreference
        {
			get => _tileDensityPreference;
            set 
			{
				_tileDensityPreference = value;
				PropChanged("tileDensityPreference");
			}
        }


		public event PropertyChangedEventHandler PropertyChanged;

		public Chapter( string name ) : base()
		{
			dataName = name;
			GUID = Guid.NewGuid();
			isEmpty = false;
			noFlavorText = true;
			flavorBookData = new TextBookData();
			flavorBookData.pages.Add( "This optional text is shown when any Tile in this Chapter is first explored. It is only shown once." );
			exploreTrigger = triggeredBy = triggerName = "None";
			tileObserver = new ObservableCollection<ITile>();
			//randomTilePool = new ObservableCollection<int>();
			randomInteractionGroup = "None";
			if (name == "Start")
			{
				isPreExplored = true;
				isStart = true;
				isEmpty = true;
			}
			else
			{
				isPreExplored = false;
				isStart = false;
			}
			usesRandomGroups = false;
			isDynamic = false;
			attachHint = "None";
			attachTileHint = 0;
		}

		//public Chapter CreateDefault()
		//{
		//	return new Chapter( "Start" )
		//	{
		//		isEmpty = true,
		//		isPreExplored = true,
		//		isDynamic = false
		//	};
		//}

		public void AddTile( BaseTile t )
		{
			tileObserver.Add( t );
		}

		public void RemoveTile( BaseTile t )
		{
			tileObserver.Remove( t );
		}

		public void RenameTrigger( string oldName, string newName, bool isJourney )
		{
			if ( triggerName == oldName )
				triggerName = newName;
			if ( triggeredBy == oldName )
				triggeredBy = newName;
			if ( exploreTrigger == oldName )
				exploreTrigger = newName;

			//rename tiles

		}

		public void ToJourneyTile()
		{
			tileObserver = new ObservableCollection<ITile>();
		}

		public void ToSquareTile()
		{
			tileObserver = new ObservableCollection<ITile>();
			//tileObserver.Add(new SquareTile());
		}

		public void ToBattleTile()
		{
			tileObserver = new ObservableCollection<ITile>();
			for ( int i = 0; i < 10; i++ )
			{
				tileObserver.Add( new BattleTile() );
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
