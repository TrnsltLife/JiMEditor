using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	/// <summary>
	/// A standalone mission, or a single mission in a campaign
	/// </summary>
	public class Scenario : Translatable, INotifyPropertyChanged
	{
		override public string TranslationKeyName() { return "scenario"; }
		override public string TranslationKeyPrefix() { return "scenario."; }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("scenario.scenarioName", () => this.scenarioName),
				new TranslationAccessor("scenario.instructions", () => this.specialInstructions),
				new TranslationAccessor("scenario.introduction", () => this.introBookData.pages[0])
			};
			translationAccessors = list;
		}

		string _scenarioName, _fileName, _objectiveName, _fileVersion, _specialInstructions, _coverImage;
		bool _isDirty, _scenarioTypeJourney, _useTileGraphics;
		int _threatMax, _loreReward, _xpReward, _shadowFear, _loreStartValue, _xpStartValue;
		int[] _wallTypes;
		Guid _scenarioGUID, _campaignGUID;
		//titleChangedToken is ONLY used to trigger the window Title converter
		Tuple<bool, string, Guid, ProjectType> _titleChangedToken;
		public int[] wallTypes
		{
			get => _wallTypes;
			set
			{
				_wallTypes = value;
				PropChanged( "wallTypes" );
			}
		}

		public string saveDate { get; set; }
		#region propchanged
		public Tuple<bool, string, Guid, ProjectType> titleChangedToken
		{
			get => _titleChangedToken;
			set
			{
				if ( _titleChangedToken != value )
				{
					_titleChangedToken = value;
					PropChanged( "titleChangedToken" );
				}
			}
		}
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
		public bool scenarioTypeJourney
		{
			get => _scenarioTypeJourney;
			set
			{
				if ( value != _scenarioTypeJourney )
				{
					_scenarioTypeJourney = value;
					PropChanged( "scenarioTypeJourney" );
				}
			}
		}
		/// <summary>
		/// just the file NAME, not the path
		/// </summary>
		public string fileName
		{
			get => _fileName;
			set
			{
				if ( value != _fileName )
				{
					_fileName = value;
					PropChanged( "fileName" );
				}
			}
		}
		public string fileVersion
		{
			get => _fileVersion;
			set
			{
				if ( value != _fileVersion )
				{
					_fileVersion = value;
					PropChanged( "fileVersion" );
				}
			}
		}
		public string scenarioName
		{
			get => _scenarioName;
			set
			{
				if ( _scenarioName != value )
				{
					_scenarioName = value;
					PropChanged( "scenarioName" );
				}
			}
		}
		public string objectiveName
		{
			get => _objectiveName;
			set
			{
				_objectiveName = value;
				PropChanged( "objectiveName" );
			}
		}
		public int threatMax
		{
			get => _threatMax;
			set
			{
				if ( value != _threatMax )
				{
					_threatMax = value;
					PropChanged( "threatMax" );
				}
			}
		}
		public bool threatNotUsed { get; set; }
		public ProjectType projectType { get; set; }
		public TextBookData introBookData { get; set; }
		public int loreReward
		{
			get => _loreReward;
			set { _loreReward = value; PropChanged( "loreReward" ); }
		}
		public int loreStartValue
		{
			get => _loreStartValue;
			set { _loreStartValue = value; PropChanged( "loreStartValue" ); }
		}
		public int xpReward
		{
			get => _xpReward;
			set { _xpReward = value; PropChanged( "xpReward" ); }
		}
		public int xpStartValue
		{
			get => _xpStartValue;
			set { _xpStartValue = value; PropChanged("xpStartValue"); }
		}
		public int shadowFear
		{
			get => _shadowFear;
			set { _shadowFear = value; PropChanged( "shadowFear" ); }
		}
		public Guid scenarioGUID
		{
			get => _scenarioGUID;
			set
			{
				_scenarioGUID = value;
				PropChanged( "scenarioGUID" );
			}
		}
		public Guid campaignGUID
		{
			get => _campaignGUID;
			set
			{
				_campaignGUID = value;
				PropChanged( "campaignGUID" );
			}
		}
		public string specialInstructions
		{
			get => _specialInstructions;
			set
			{
				_specialInstructions = value;
				PropChanged( "specialInstructions" );
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

		public bool useTileGraphics
		{
			get => _useTileGraphics;
			set
			{
				_useTileGraphics = value;
				PropChanged( "useTileGraphics" );
			}
		}
		#endregion

		public Dictionary<string, bool> scenarioEndStatus { get; set; }
		public ObservableCollection<IInteraction> interactionObserver { get; set; }
		public ObservableCollection<Trigger> triggersObserver { get; set; }
		public ObservableCollection<Objective> objectiveObserver { get; set; }
		public ObservableCollection<MonsterModifier> monsterModifierObserver { get; set; }
		public ObservableCollection<MonsterActivations> activationsObserver { get; set; }
		//public List<MonsterActivations> filteredActivationsObserver => activationsObserver.Where(a => collectionObserver.Contains(a.collection)).ToList();
		public ObservableCollection<Translation> translationObserver { get; set; }
		public ObservableCollection<TextBookData> resolutionObserver { get; set; }
		public ObservableCollection<Threat> threatObserver { get; set; }
		public ObservableCollection<Chapter> chapterObserver { get; set; }
		public ObservableCollection<Collection> collectionObserver { get; set; }
		public ObservableCollection<int> globalTilePool { get; set; }

		public ObservableCollection<int> filteredGlobalTilePool { get; set; }

        public void AddObserverChangedHandler(NotifyCollectionChangedEventHandler handler)
        {
            // Fires up the handler whenever any of the Observable Collections change
            interactionObserver.CollectionChanged += handler;
            triggersObserver.CollectionChanged += handler;
            objectiveObserver.CollectionChanged += handler;
			monsterModifierObserver.CollectionChanged += handler;
            activationsObserver.CollectionChanged += handler;
            resolutionObserver.CollectionChanged += handler;
            threatObserver.CollectionChanged += handler;
            chapterObserver.CollectionChanged += handler;
            collectionObserver.CollectionChanged += handler;
            globalTilePool.CollectionChanged += handler;
            filteredGlobalTilePool.CollectionChanged += handler;
        }

		public static bool IsBattleTile(int t)
        {
			return (t == 998 || t == 999);
        }

		private void AddCollectionChangedLambda()
		{
			globalTilePool.CollectionChanged += (object s, NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
                {
					if (e.NewItems != null)
					{
						foreach (var item in e.NewItems)
						{
							if (scenarioTypeJourney ? !IsBattleTile((int)item) && collectionObserver.Contains(Collection.FromTileNumber((int)item))
													:  IsBattleTile((int)item) && collectionObserver.Contains(Collection.FromTileNumber((int)item)))
							{
								filteredGlobalTilePool.Add((int)item);
							}
						}
					}
				}
				else if(e.Action == NotifyCollectionChangedAction.Remove)
                {
					if (e.OldItems != null)
					{
						foreach (var item in e.OldItems)
						{
							filteredGlobalTilePool.Remove((int)item);
						}
					}
				}
				else if(e.Action == NotifyCollectionChangedAction.Reset)
                {
					filteredGlobalTilePool.Clear();
				}
				else if(e.Action == NotifyCollectionChangedAction.Replace && e.NewStartingIndex >= 0)
                {
					int i = 0;
					foreach (var item in e.NewItems)
					{
						if (e.NewStartingIndex + i >= filteredGlobalTilePool.Count)
						{
							filteredGlobalTilePool.Append((int)item);
						}
						else
						{
							filteredGlobalTilePool[e.NewStartingIndex + i] = (int)item;
						}
						i++;
					}
                }
			};
		}

		public void RefilterGlobalTilePool()
        {
			filteredGlobalTilePool.Clear();
			//If Journey Map is selected, filter only the tiles in the currently selected Collections
			//If Battle Map is selected, fitler only tiles 998 and 999 which stand for the two Battle Map tiles
			var newList = new ObservableCollection<int>(globalTilePool
				.Where(tileNumber => scenarioTypeJourney ? !IsBattleTile(tileNumber) && collectionObserver.Contains(Collection.FromTileNumber(tileNumber)) 
														 :  IsBattleTile(tileNumber) && collectionObserver.Contains(Collection.FromTileNumber(tileNumber))).ToList());
			filteredGlobalTilePool.Clear();
			foreach (var item in newList)
			{
				filteredGlobalTilePool.Add(item);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Scenario() : this( "Click Text To Edit Scenario Title" )
		{
			CreateDefaults();
		}

		public Scenario( string name )
		{
			scenarioName = name;
			isDirty = true;
			projectType = ProjectType.Standalone;
			titleChangedToken = new Tuple<bool, string, Guid, ProjectType>( true, string.Empty, Guid.NewGuid(), ProjectType.Standalone );
			scenarioTypeJourney = true;

			interactionObserver = new ObservableCollection<IInteraction>();
			triggersObserver = new ObservableCollection<Trigger>();
			objectiveObserver = new ObservableCollection<Objective>();
			monsterModifierObserver = new ObservableCollection<MonsterModifier>();
			activationsObserver = new ObservableCollection<MonsterActivations>();
			translationObserver = new ObservableCollection<Translation>();
			resolutionObserver = new ObservableCollection<TextBookData>();
			threatObserver = new ObservableCollection<Threat>();
			chapterObserver = new ObservableCollection<Chapter>();
			collectionObserver = new ObservableCollection<Collection>();
			collectionObserver.Add(Collection.CORE_SET);
			globalTilePool = new ObservableCollection<int>( Utils.LoadTiles() );
			filteredGlobalTilePool = new ObservableCollection<int>();
			RefilterGlobalTilePool();
			AddCollectionChangedLambda();
			scenarioEndStatus = new Dictionary<string, bool>();
			Utils.LoadHexData();
		}

		/// <summary>
		/// Load in data from FileManager
		/// </summary>
		public static Scenario CreateInstance( FileManager fm )
		{
			Scenario s = new Scenario();
			s.scenarioName = fm.scenarioName;
			s.fileName = fm.fileName;
			s.fileVersion = fm.fileVersion;
			s.saveDate = fm.saveDate;
			s.projectType = fm.projectType;
			s.objectiveName = fm.objectiveName;
			s.interactionObserver = new ObservableCollection<IInteraction>( fm.interactions );
			s.triggersObserver = new ObservableCollection<Trigger>( fm.triggers );
			s.objectiveObserver = new ObservableCollection<Objective>( fm.objectives );
			if(fm.monsterModifiers != null)
            {
				s.monsterModifierObserver = new ObservableCollection<MonsterModifier>(fm.monsterModifiers);
            }
			if (fm.activations != null)
			{
				s.activationsObserver = new ObservableCollection<MonsterActivations>(fm.activations);
			}
			if (fm.translations != null)
			{
				s.translationObserver = new ObservableCollection<Translation>(fm.translations);
			}
			s.resolutionObserver = new ObservableCollection<TextBookData>( fm.resolutions );
			s.threatObserver = new ObservableCollection<Threat>( fm.threats );
			s.chapterObserver = new ObservableCollection<Chapter>( fm.chapters );
			s.collectionObserver = new ObservableCollection<Collection>(fm.collections);
			if(!s.collectionObserver.Contains(Collection.CORE_SET))
            {
				s.collectionObserver.Add(Collection.CORE_SET);
            }
			s.globalTilePool = new ObservableCollection<int>( fm.globalTiles );
			s.filteredGlobalTilePool = new ObservableCollection<int>(s.globalTilePool.Where(tileNumber => s.collectionObserver.Contains(Collection.FromTileNumber(tileNumber))).ToList());
			s.AddCollectionChangedLambda();
			s.introBookData = fm.introBookData;
			s.threatMax = fm.threatMax;
			s.threatNotUsed = fm.threatNotUsed;
			s.scenarioTypeJourney = fm.scenarioTypeJourney;
			s.loreReward = fm.loreReward;
			s.xpReward = fm.xpReward;
			s.shadowFear = fm.shadowFear;
			s.loreStartValue = fm.loreStartValue;
			s.xpStartValue = fm.xpStartValue;
			s.scenarioGUID = fm.scenarioGUID;
			s.campaignGUID = fm.campaignGUID;
			s.specialInstructions = fm.specialInstructions ?? "";
			s.coverImage = fm.coverImage;
			s.useTileGraphics = fm.useTileGraphics;
			s.scenarioEndStatus = new Dictionary<string, bool>( fm.scenarioEndStatus );

			if ( s.scenarioGUID.ToString() == "00000000-0000-0000-0000-000000000000" )
				s.scenarioGUID = Guid.NewGuid();

			s.AddDefaultActivations();
			s.AddDefaultMonsterModifiers();

			return s;
		}

		/// <summary>
		/// Triggers the window Title binding converter to update, sets isDirty=false
		/// </summary>
		public void TriggerTitleChange( bool dirty = false )
		{
			isDirty = dirty;
			titleChangedToken = new Tuple<bool, string, Guid, ProjectType>( isDirty, fileName, Guid.NewGuid(), projectType );
		}

		void CreateDefaults()
		{
			scenarioGUID = Guid.NewGuid();
			campaignGUID = Guid.NewGuid();
			specialInstructions = "";
			coverImage = null;
			threatMax = 60;
			scenarioTypeJourney = true;
			objectiveName = "None";
			loreReward = loreStartValue = xpReward = xpStartValue = 0;
			shadowFear = 2;
			fileVersion = Utils.formatVersion;
			useTileGraphics = true;

			introBookData = new TextBookData( "Default Introduction Text" );
			introBookData.pages.Add( "Default Introduction text.\n\nThis text is displayed at the beginning of the Scenario to describe the mission and Objectives.\n\nScenarios have one Introduction Text." );

			TextBookData data = new TextBookData( "Default Resolution" );
			data.pages.Add( "Default resolution text.  This text is displayed when the Scenario has ended.\n\nDifferent Resolutions can be shown based on a Trigger that gets set during the Scenario, depending on player actions.\n\nScenarios have at least one Resolution." );
			data.triggerName = "Scenario Ended";
			AddResolution( data, true );

			//Always have one EMPTY Trigger / Objective / Interaction
			triggersObserver.Add( Trigger.EmptyTrigger() );
			//triggersObserver.Add( Trigger.RandomTrigger() );
			objectiveObserver.Add( Objective.EmptyObjective() );
			interactionObserver.Add( NoneInteraction.EmptyInteraction() );
			AddTrigger( "Scenario Ended" );
			AddTrigger( "Objective Complete" );
			//AddInteraction( Interaction.EmptyInteraction() );

			//default objective - always at least 1 in the scenario
			Objective obj = new Objective( "Default Objective" ) { triggerName = "Objective Complete" };
			objectiveObserver.Add( obj );

			//Add the default enemy activations
			AddDefaultActivations();

			//Add the default monster modifiers
			AddDefaultMonsterModifiers();

			//starting chapter - always at least one in the scenario
			Chapter chapter = new Chapter( "Start" ) { isEmpty = true };
			chapterObserver.Add( chapter );

			wallTypes = new int[22];
			for ( int i = 0; i < 22; i++ )
				wallTypes[i] = 0;//0=none, 1=wall, 2=river
		}

		public void AddDefaultMonsterModifiers()
		{
			//Remove default monster modifiers if they exist
			for (int i = monsterModifierObserver.Count - 1; i >= 0; i--)
			{
				if (monsterModifierObserver[i].id < MonsterModifier.START_OF_CUSTOM_MODIFIERS)
				{
					monsterModifierObserver.RemoveAt(i);
				}
			}

			//Add the default monster modifiers
			int j = 0;
			foreach (MonsterModifier modifier in MonsterModifier.Values)
			{
				monsterModifierObserver.Insert(j, modifier);
				j++;
			}
		}

		public void AddDefaultActivations()
        {
			//Remove default activations if they exist
			for(int i = activationsObserver.Count - 1; i >= 0; i-- )
            {
                if (activationsObserver[i].id < MonsterActivations.START_OF_CUSTOM_ACTIVATIONS)
                {
					activationsObserver.RemoveAt(i);
                }
            }

			//Add the default enemy activations
			int j = 0;
			foreach (DefaultActivations defAct in Utils.defaultActivations)
			{
				MonsterActivations act = new MonsterActivations(defAct);
				activationsObserver.Insert(j, act);
				j++;
			}
		}

		public void AddDefaultTerrainInteractions()
        {
			//Add the default terrain interactions
			foreach (InteractionBase terrainInteraction in Utils.defaultTerrainInteractions)
			{
				if (!interactionObserver.Contains(terrainInteraction))
				{
					interactionObserver.Add(terrainInteraction);
				}
			}
		}

		public void RemoveDefaultTerrainInteractions()
		{
			//Remove the default terrain interactions
			foreach (InteractionBase terrainInteraction in Utils.defaultTerrainInteractions)
			{
				if (interactionObserver.Contains(terrainInteraction))
				{
					interactionObserver.Remove(terrainInteraction);
				}
			}
		}

		public Boolean IsCollectionEnabled(Collection collection)
        {
			return collectionObserver.Contains(collection);
        }

		public Boolean IsCollectionEnabled(string name)
        {
			return IsCollectionEnabled(Collection.FromName(name));
        }

		public void WipeChapters()
		{
			var listCopy = chapterObserver.ToList();
			foreach (var c in listCopy)
			{
				RemoveChapter(c);
			}
			chapterObserver.Clear();
			Chapter chapter = new Chapter( "Start" ) { isEmpty = true };
			chapterObserver.Add( chapter );
		}

		public void RemoveChapter(Chapter c)
        {
			foreach (var tile in c.tileObserver)
			{
				globalTilePool.Add(tile.idNumber);
			}
			TileSorter sorter = new TileSorter();
			List<int> foo = globalTilePool.ToList();
			foo.Sort(sorter);
			globalTilePool.Clear();
			foreach (int s in foo)
			{
				globalTilePool.Add(s);
			}
			RemoveData(c);
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public void AddChapter( Chapter chapter )
		{
			chapterObserver.Add( chapter );
		}

		public void AddInteraction( IInteraction interaction )
		{
			switch ( interaction.interactionType )
			{
				case InteractionType.Branch:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Text:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Threat:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.StatTest:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Decision:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Darkness:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.MultiEvent:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Persistent:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Conditional:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Dialog:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Replace:
					interactionObserver.Add( interaction );
					break;
				case InteractionType.Reward:
					interactionObserver.Add( interaction );
					break;
				default:
					throw new Exception( "Interaction type not supported: " + interaction.interactionType );
			}

			//sort by name
			/*
			List<IInteraction> sorted = interactionObserver.OrderBy( key => key.dataName != "None" ).ThenBy( key => key.dataName ).ToList();
			for ( int i = 0; i < sorted.Count; i++ )
				interactionObserver[i] = sorted[i];
			*/
		}

		public bool AddTrigger( string name, bool isMulti = false )
		{
			if ( ( from Trigger foo in triggersObserver where foo.dataName == name select foo ).Count() == 0 )
			{
				Trigger t = new Trigger( name )
				{
					isMultiTrigger = isMulti
				};
				triggersObserver.Add( t );

				//sort by name
				/*
				List<Trigger> trigsorted = triggersObserver.OrderBy( key => key.dataName != "None" ).ThenBy( key => key.dataName ).ToList();
				for ( int i = 0; i < trigsorted.Count; i++ )
					triggersObserver[i] = trigsorted[i];
				*/

				return true;
			}
			return false;
		}

		public void AddObjective( Objective objective )
		{
			objectiveObserver.Add( objective );

			//sort by name
			/*
			List<Objective> objsorted = objectiveObserver.OrderBy( key => key.dataName != "None" ).ThenBy( key => key.dataName ).ToList();
			for ( int i = 0; i < objsorted.Count; i++ )
				objectiveObserver[i] = objsorted[i];
			*/
		}

		public void AddMonsterModifier(MonsterModifier modifier)
		{
			monsterModifierObserver.Add(modifier);
		}

		public void AddActivations(MonsterActivations activations)
		{
			activationsObserver.Add(activations);
		}

		public void AddTranslation(Translation translation)
        {
			translationObserver.Add(translation);
        }

		public Dictionary<string, TranslationItem> CollectTranslationItemsAsDictionary()
        {
			List<TranslationItem> defaultTranslationList = CollectAllTranslationItems();
			defaultTranslationList.Sort();
			Dictionary<string, TranslationItem> defaultTranslation = new Dictionary<string, TranslationItem>();
			foreach (var item in defaultTranslationList)
			{
				//Console.WriteLine(item.key + " => " + item.text);
				defaultTranslation.Add(item.key, item);
			}
			return defaultTranslation;
		}

		//Collect translations from all the various game objects that can have translatable text
		public List<TranslationItem> CollectAllTranslationItems()
        {
			List<TranslationItem> defaultTranslations = new List<TranslationItem>();

			defaultTranslations.AddRange(this.CollectTranslationItems());

			foreach(var objective in objectiveObserver)
            {
				defaultTranslations.AddRange(objective.CollectTranslationItems());
			}

			foreach(var modifier in monsterModifierObserver)
            {
				if (modifier.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS)
				{
					defaultTranslations.AddRange(modifier.CollectTranslationItems());
				}
            }

			foreach(var chapter in chapterObserver)
            {
				defaultTranslations.AddRange(chapter.CollectTranslationItems());
				//grab lists of TranslationItems from all the chapters tiles and flatten them into one big list
				defaultTranslations.AddRange(chapter.tileObserver.ToList().ConvertAll(tile => ((BaseTile)tile).CollectTranslationItems()).SelectMany(list => list));
            }

			foreach(var resolution in resolutionObserver)
            {
				defaultTranslations.AddRange(resolution.CollectTranslationItems());
            }

			foreach (var activation in activationsObserver)
			{
				if (activation.id >= MonsterActivations.START_OF_CUSTOM_ACTIVATIONS)
				{
					defaultTranslations.AddRange(activation.CollectTranslationItems());
					//grab lists of TranslationItems from all the chapters tiles and flatten them into one big list
					defaultTranslations.AddRange(activation.activations.ToList().ConvertAll(item =>
					{
						item.translationKeyParents = activation.dataName;
						return item.CollectTranslationItems();
					}).SelectMany(list => list));
				}
			}

			foreach (var interaction in interactionObserver)
            {
				defaultTranslations.AddRange(((InteractionBase)interaction).CollectTranslationItems());

				if(interaction is ThreatInteraction)
                {
					ThreatInteraction threat = (ThreatInteraction)interaction;
					threat.CheckMonsterNumbering(this.translationObserver);
					//Collect the monster translations
					defaultTranslations.AddRange(threat.monsterCollection.ToList().ConvertAll(item =>
					{
						item.translationKeyParents = threat.dataName;
						return item.CollectTranslationItems();
					}).SelectMany(list => list));

					/*
					List<TranslationAccessor> list = new List<TranslationAccessor>();
					foreach (var monster in threat.monsterCollection)
					{
						list.Add(new TranslationAccessor("event.{1}.{0}.monster." + monster.index + ".name", () => monster.dataName));
					}
					translationAccessors.AddRange(list);
					*/
				}
			}

			//remove default translations with empty value
			defaultTranslations = defaultTranslations.FindAll(it => !String.IsNullOrWhiteSpace(it.text)).ToList();

			return defaultTranslations;
        }


		/// <summary>
		/// adds a Resolution to Scenario AND adds EndStatus bool for it
		/// </summary>
		public bool AddResolution( TextBookData data, bool success )
		{
			if ( ( from foo in resolutionObserver where foo.dataName == data.dataName select foo ).Count() == 0 )
			{
				resolutionObserver.Add( data );
				if ( !scenarioEndStatus.ContainsKey( data.dataName ) )
					scenarioEndStatus.Add( data.dataName, success );//create key
				else
					scenarioEndStatus[data.dataName] = success;//or update key
				return true;
			}
			return false;
		}

		/// <summary>
		/// removes scenario end key/values that are no longer in use
		/// </summary>
		public void PruneScenarioEnd()
		{
			var prune = scenarioEndStatus.Where( x => resolutionObserver.Any( y => y.dataName == x.Key ) );
			scenarioEndStatus = new Dictionary<string, bool>();
			foreach ( var foo in prune )
			{
				scenarioEndStatus.Add( foo.Key, foo.Value );
			}
		}

		public void RemoveData<T>( T item )
		{
			if (item is Translation)
			{
				translationObserver.Remove(item as Translation);
				return;
			}



			if ( ( (ICommonData)item ).isEmpty )
				return;

			if (item is IInteraction)
				interactionObserver.Remove(item as IInteraction);
			else if (item is Trigger)
				triggersObserver.Remove(item as Trigger);
			else if (item is Objective)
				objectiveObserver.Remove(item as Objective);
			else if (item is MonsterModifier)
			{
				RemoveMonsterModifiersInUse(item as MonsterModifier);
				monsterModifierObserver.Remove(item as MonsterModifier);
			}
			else if (item is MonsterActivations)
			{
				RemoveActivationsInUse((item as MonsterActivations).id);
				activationsObserver.Remove(item as MonsterActivations);
			}
			else if (item is TextBookData)
				resolutionObserver.Remove(item as TextBookData);
			else if (item is Threat)
				threatObserver.Remove(item as Threat);
			else if (item is Chapter)
				chapterObserver.Remove(item as Chapter);
		}

		/// <summary>
		/// Renames the Trigger (if new name doesn't exist) and updates all data collections so they point to the new name
		/// </summary>
		public bool RenameTrigger( string oldName, string newName, bool isMulti )
		{
			//bail out if new name already exists
			if ( triggersObserver.Count( t => t.dataName == newName ) > 0 )
				return false;

			foreach ( var obj in objectiveObserver )
				obj.RenameTrigger( oldName, newName );

			foreach ( var obj in resolutionObserver )
				if ( obj.triggerName == oldName )
					obj.triggerName = newName;

			foreach ( var obj in interactionObserver )
				obj.RenameTrigger( oldName, newName );

			foreach ( var obj in threatObserver )
				if ( obj.triggerName == oldName )
					obj.triggerName = newName;

			foreach ( var obj in chapterObserver )
				obj.RenameTrigger( oldName, newName, scenarioTypeJourney );

			//finally rename the trigger object itself
			triggersObserver.Where( t => t.dataName == oldName ).First().isMultiTrigger = isMulti;
			triggersObserver.Where( t => t.dataName == oldName ).First().dataName = newName;

			return true;
		}

		/// <summary>
		/// Renames the Translation to a new langCode (if new langCode doesn't already exist)
		/// </summary>
		public bool RenameTranslation(string oldLangCode, string newLangCode)
		{
			//bail out if new name already exists
			if (translationObserver.Count(t => t.dataName == newLangCode) > 0)
				return false;

			//finally rename the translation object itself
			translationObserver.Where(t => t.dataName == oldLangCode).First().dataName = newLangCode;

			return true;
		}

		/// <summary>
		/// Check if named trigger is being used anywhere
		/// </summary>
		public Tuple<string, string> IsTriggerUsed( string name )
		{
			if ( triggersObserver.Count > 0 )
			{
				var used = interactionObserver.IsTriggerUsed( name );
				if ( used != null )
					return used;
				used = triggersObserver.IsTriggerUsed( name );
				if ( used != null )
					return used;
				used = objectiveObserver.IsTriggerUsed( name );
				if ( used != null )
					return used;
				used = resolutionObserver.IsTriggerUsed( name );
				if ( used != null )
					return used;

				//Threats no longer use Triggers, they use Events
				//used = threatObserver.IsTriggerUsed( name );
				//if ( used != null )
				//	return used;

				used = chapterObserver.IsTriggerUsed( name );
				if ( used != null )
					return used;
			}
			return null;
		}

		public void RemoveActivationsInUse(int activationId)
        {
			foreach(ThreatInteraction threat in interactionObserver.Where(it => it.interactionType == InteractionType.Threat))
            {
				foreach(Monster monster in threat.monsterCollection)
                {
					if(monster.activationsId == activationId)
                    {
						monster.activationsId = monster.id; //reset to using the default activation for this mosnter type. We could also reset to None, which is -1
                    }
                }
            }
        }

		public void RemoveMonsterModifiersInUse(MonsterModifier mod)
		{
			foreach (ThreatInteraction threat in interactionObserver.Where(it => it.interactionType == InteractionType.Threat))
			{
				foreach (Monster monster in threat.monsterCollection)
				{
					monster.modifierList.Remove(mod);
				}
			}
		}

		/// <summary>
		/// Checks for duplicate name usage within same object type
		/// </summary>
		public bool IsDuplicate( ICommonData data )
		{
			if ( data is IInteraction )
			{
				for ( int i = 0; i < interactionObserver.Count; i++ )
				{
					if ( interactionObserver[i].dataName == data.dataName
					&& interactionObserver[i].GUID != data.GUID )
						return true;
				}
			}
			else if ( data is Trigger )
			{
				for ( int i = 0; i < triggersObserver.Count; i++ )
				{
					if ( triggersObserver[i].dataName == data.dataName
					&& triggersObserver[i].GUID != data.GUID )
						return true;
				}
			}
			else if ( data is Objective )
			{
				for ( int i = 0; i < objectiveObserver.Count; i++ )
				{
					if ( objectiveObserver[i].dataName == data.dataName
					&& objectiveObserver[i].GUID != data.GUID )
						return true;
				}
			}
			else if (data is MonsterModifier)
			{
				for (int i = 0; i < monsterModifierObserver.Count; i++)
				{
					if (monsterModifierObserver[i].dataName == data.dataName
					&& monsterModifierObserver[i].GUID != data.GUID)
						return true;
				}
			}
			else if ( data is TextBookData )
			{
				for ( int i = 0; i < resolutionObserver.Count; i++ )
				{
					if ( resolutionObserver[i].dataName == data.dataName
					&& resolutionObserver[i].GUID != data.GUID )
						return true;
				}
			}
			else if ( data is Threat )
			{
				for ( int i = 0; i < threatObserver.Count; i++ )
				{
					if ( threatObserver[i].dataName == data.dataName
					&& threatObserver[i].GUID != data.GUID )
						return true;
				}
			}
			else if ( data is Chapter )
			{
				for ( int i = 0; i < chapterObserver.Count; i++ )
				{
					if ( chapterObserver[i].dataName == data.dataName
					&& chapterObserver[i].GUID != data.GUID )
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Updates any token or threat that references this interaction to use the new name.
		/// Updates Tokens if the event tokentype/personType changed.
		/// Updates Tokens if event is no longer a token interaction.
		/// Removes Event from Threat if Event becomes a token interaction.
		/// </summary>
		public void UpdateEventReferences( string oldName, IInteraction interaction )
		{
			//go through all the assigned tokens in hextiles and update their tokentype and triggerName
			foreach ( var chapter in chapterObserver )
			{
				foreach ( var tile in chapter.tileObserver )
				{
					if ( tile is HexTile hexTile )
					{
						foreach ( var token in hexTile.tokenList )
						{
							if ( token.triggerName == oldName )
							{
								//update token type
								token.tokenType = interaction.tokenType;
								//update person type
								token.personType = interaction.personType;
								//update terrain type
								token.terrainType = interaction.terrainType;
								//rename
								token.triggerName = interaction.dataName;
								//remove event if it's no longer a token interaction
								if ( !interaction.isTokenInteraction )
								{
									token.triggerName = "None";
									hexTile.tokenList.Remove( token );
									break;
								}
							}
						}
					}
				}
			}

			//rename scenario threats that references this event
			foreach ( var threat in threatObserver )
			{
				if ( threat.triggerName == oldName )
				{
					threat.triggerName = interaction.dataName;
					//remove event if it's become a token interaction
					if ( interaction.isTokenInteraction )
						threat.triggerName = "None";
				}
			}
		}
	}
}
