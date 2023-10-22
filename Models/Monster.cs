using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class Monster : Translatable, INotifyPropertyChanged, ICommonData
	{
		override public string TranslationKeyName() { return index.ToString(); }
		override public string TranslationKeyPrefix() { return String.Format("event.enemy.{1}.monster.{0}.", TranslationKeyName(), translationKeyParents); }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("event.enemy.{1}.monster.{0}.name", () => this.dataName),
			};
			translationAccessors = list;
		}

		int Light { get { return 2; } }
		int Medium { get { return 3; } }
		int Heavy { get { return 4; } }

		string _dataName, /*_name,*/ _bonuses;
		int _index, _id, _activationsId, _count, _health, _shieldValue, _sorceryValue, _moveA, _moveB, _groupLimit, _figureLimit, _damage, _loreReward, _movementValue, _maxMovementValue;
		bool _isRanged, _isFearsome, _isLarge, _isBloodThirsty, _isArmored, _isElite, _defaultStats, _isEasy, _isNormal, _isHard;
		int[] _cost;
		string[] _moveSpecial, _tag, _special;

		ObservableCollection<MonsterModifier> _modifierList = new ObservableCollection<MonsterModifier>();

		public Guid GUID { get; set; }

		public int index //used to keep track of the order in the list in a ThreatEvent, as a key for the translation string
		{
			get => _index;
			set
			{
				if (value != _index)
				{
					_index = value;
					NotifyPropertyChanged("index");
				}
			}
		}

		public int id //refers to the monsterId that identifies the monster as a Ruffian, Orc, Varg, etc.
		{
			get => _id;
			set
			{
				if (value != _id)
				{
					_id = value;
					NotifyPropertyChanged("id");
				}
			}
		}

		public int activationsId
		{
			get => _activationsId;
			set
			{
				if (value != _activationsId)
				{
					_activationsId = value;
					NotifyPropertyChanged("activationsId");
				}
			}
		}

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

		/*
		public string name
		{
			get => _name;
			set
			{
				if (value != _name)
				{
					_name = value;
					NotifyPropertyChanged("name");
				}
			}
		}
		*/

		public bool isEmpty { get; set; }
		public string triggerName { get; set; }

		public string bonuses
		{
			get
			{
				string b = string.Empty;
				/*
				if ( isLarge )
					b += "Large, ";
				if ( isBloodThirsty )
					b += "BloodThirsty, ";
				if ( isArmored )
					b += "Armored, ";
				*/
				b = string.Join(", ", modifierList.ToList().ConvertAll(it => it.name));
				if ( string.IsNullOrEmpty( b ) )
					return "No Bonuses";
				else
					return b;
			}
			set
			{
				if ( value != _bonuses )
				{
					_bonuses = value;
					NotifyPropertyChanged( "bonuses" );
				}
			}
		}

		public int health
		{
			get => _health;
			set
			{
				if ( value != _health )
				{
					_health = value;
					NotifyPropertyChanged( "health" );
				}
			}
		}

		public int shieldValue
		{
			get => _shieldValue;
			set
			{
				if ( value != _shieldValue )
				{
					_shieldValue = value;
					_shieldValue = Math.Min( _shieldValue, _health );
					NotifyPropertyChanged( "shieldValue" );
				}
			}
		}

		public int sorceryValue
		{
			get => _sorceryValue;
			set
			{
				if ( value != _sorceryValue )
				{
					_sorceryValue = value;
					_sorceryValue = Math.Min( _sorceryValue, _health );
					NotifyPropertyChanged( "sorceryValue" );
				}
			}
		}

		public int moveA
		{
			get => _moveA;
			set
			{
				if (value != _moveA)
				{
					_moveA = value;
					NotifyPropertyChanged("moveA");
				}
			}
		}

		public int moveB
		{
			get => _moveB;
			set
			{
				if (value != _moveB)
				{
					_moveB = value;
					NotifyPropertyChanged("moveB");
				}
			}
		}

		public string[] moveSpecial
		{
			get => _moveSpecial;
			set
			{
				if (value != _moveSpecial)
				{
					_moveSpecial = value;
					NotifyPropertyChanged("moveSpecial");
					specialAbility = string.Join(", ", moveSpecial);
				}
			}
		}

		public bool isRanged
		{
			get => _isRanged;
			set
			{
				_isRanged = value;
				NotifyPropertyChanged("isRanged");
			}
		}

		public int groupLimit
		{
			get => _groupLimit;
			set
			{
				if (value != _groupLimit)
				{
					_groupLimit = value;
					NotifyPropertyChanged("groupLimit");
				}
			}
		}

		public int figureLimit
		{
			get => _figureLimit;
			set
			{
				if (value != _figureLimit)
				{
					_figureLimit = value;
					NotifyPropertyChanged("figureLimit");
				}
			}
		}

		public int[] cost
		{
			get => _cost;
			set
			{
				if (value != _cost)
				{
					_cost = value;
					NotifyPropertyChanged("cost");
				}
			}
		}

		public string[] tag
		{
			get => _tag;
			set
			{
				if (value != _tag)
				{
					_tag = value;
					NotifyPropertyChanged("tag");
				}
			}
		}

		public int damage
		{
			get => _damage;
			set
			{
				if ( value != _damage )
				{
					_damage = value;
					NotifyPropertyChanged( "damage" );
				}
			}
		}

		public bool isFearsome
		{
			get => _isFearsome;
			set
			{
				_isFearsome = value;
				NotifyPropertyChanged("isFearsome");
			}
		}

		public string[] special
		{
			get => _special;
			set
			{
				if (value != _special)
				{
					_special = value;
					NotifyPropertyChanged("special");
					specialAbility = string.Join(", ", special);
				}
			}
		}

		public bool isLarge
		{
			get => _isLarge;
			set
			{
				_isLarge = value;
				NotifyPropertyChanged( "isLarge" );
				bonuses = bonuses;
				/*
				if ( _isArmored || _isBloodThirsty || _isLarge )
					isElite = true;
				else
					isElite = false;
				*/
			}
		}
		public bool isBloodThirsty
		{
			get => _isBloodThirsty;
			set
			{
				_isBloodThirsty = value;
				NotifyPropertyChanged( "isBloodThirsty" );
				bonuses = bonuses;
				if ( _isArmored || _isBloodThirsty || _isLarge )
					isElite = true;
				else
					isElite = false;
			}
		}
		public bool isArmored
		{
			get => _isArmored;
			set
			{
				_isArmored = value;
				NotifyPropertyChanged( "isArmored" );
				bonuses = bonuses;
				if ( _isArmored || _isBloodThirsty || _isLarge )
					isElite = true;
				else
					isElite = false;
			}
		}
		public bool isElite
		{
			get => _isElite;
			set
			{
				if ( value != _isElite )
				{
					_isElite = value;
					NotifyPropertyChanged( "isElite" );
				}
			}
		}

		public void updateElite()
        {
			if (modifierList.Count > 0) { isElite = true; }
			else { isElite = false; }
        }


		public int maxMovementValue
		{
			get => _maxMovementValue;
			set
			{
				_maxMovementValue = value;
				NotifyPropertyChanged( "maxMovementValue" );
			}
		}
		public int movementValue
		{
			get => _movementValue;
			set
			{
				_movementValue = value;
				NotifyPropertyChanged( "movementValue" );
			}
		}
		public int loreReward
		{
			get => _loreReward;
			set
			{
				_loreReward = value;
				NotifyPropertyChanged( "loreReward" );
			}
		}
		public bool defaultStats
		{
			get { return _defaultStats; }
			set
			{
				_defaultStats = value;
				NotifyPropertyChanged( "defaultStats" );
			}
		}
		public bool isEasy
		{
			get { return _isEasy; }
			set
			{
				_isEasy = value;
				NotifyPropertyChanged( "isEasy" );
			}
		}
		public bool isNormal
		{
			get { return _isNormal; }
			set
			{
				_isNormal = value;
				NotifyPropertyChanged( "isNormal" );
			}
		}
		public bool isHard
		{
			get { return _isHard; }
			set
			{
				_isHard = value;
				NotifyPropertyChanged( "isHard" );
			}
		}

		public string specialAbility { get; set; }
		public Ability negatedBy { get; set; }
		public MonsterType monsterType { get; set; }
		public int count
		{
			get => _count;
			set
			{
				if ( value != _count )
				{
					_count = value;
					NotifyPropertyChanged( "count" );
				}
			}
		}

		[JsonConverter(typeof(MonsterModifierListConverter))]
		public ObservableCollection<MonsterModifier> modifierList
        {
			get => _modifierList;
            set
			{
				if(value != _modifierList)
                {
					_modifierList = value;
					NotifyPropertyChanged("modifierList");
                }
			}
        }


		public event PropertyChangedEventHandler PropertyChanged;

		public Monster() { GUID = Guid.NewGuid(); }

		public Monster(int id)
		{
			GUID = Guid.NewGuid();
			//Console.WriteLine("Monster(" + id + ")");

			DefaultStats defaultStats;
			try
			{
				defaultStats = Utils.defaultStats.Where(x => x.id == id).First();
				specialAbility = string.Join(", ", defaultStats.special);
			}
			catch
			{
				//TODO Set up the rest of the enemies in enemy-defaults.json and get rid of this
				defaultStats = new DefaultStats();
				defaultStats.id = id;
				defaultStats.dataName = "";
				defaultStats.health = 5;
				defaultStats.armor = 1;
				defaultStats.sorcery = 0;
				defaultStats.moveA = 2;
				defaultStats.moveB = 4;
				defaultStats.moveSpecial = new string[0];
				defaultStats.ranged = false;
				defaultStats.groupLimit = 3;
				defaultStats.figureLimit = 6;
				defaultStats.cost = new int[] { 7, 13, 19 };
				defaultStats.tag = new string[0];
				defaultStats.speed = "medium";
				defaultStats.damage = "light";
				defaultStats.fearsome = false;
				defaultStats.special = new string[0];
			}

			if (defaultStats != null)
			{
				this.id = id;
				this.monsterType = (MonsterType)this.id;
				dataName = defaultStats.dataName;
				health = defaultStats.health;
				shieldValue = defaultStats.armor;
				sorceryValue = defaultStats.sorcery;
				moveA = defaultStats.moveA;
				moveB = defaultStats.moveB;
				moveSpecial = defaultStats.moveSpecial;
				isRanged = defaultStats.ranged;
				groupLimit = defaultStats.groupLimit;
				figureLimit = defaultStats.figureLimit;
				cost = defaultStats.cost;
				tag = defaultStats.tag;
				movementValue = defaultStats.speed == "light" ? Light - 1 : (defaultStats.speed == "medium" ? Medium - 1 : Heavy - 1);
				damage = defaultStats.damage == "light" ? Light : (defaultStats.damage == "medium" ? Medium : Heavy);
				isFearsome = defaultStats.fearsome;
				special = defaultStats.special;
			}
		}

		public Monster Clone()
		{
			Monster m = new Monster();
			m.dataName = this.dataName;
			m.GUID = Guid.NewGuid();
			m.bonuses = this.bonuses;
			m.index = -1;
			m.id = this.id;
			m.activationsId = this.activationsId;
			m.count = this.count;
			m.health = this.health;
			m.shieldValue = this.shieldValue;
			m.sorceryValue = this.sorceryValue;
			m.moveA = this.moveA;
			m.moveB = this.moveB;
			m.groupLimit = this.groupLimit;
			m.figureLimit = this.figureLimit;
			m.damage = this.damage;
			m.loreReward = this.loreReward;
			m.movementValue = this.movementValue;
			m.maxMovementValue = this.maxMovementValue;
			m.isRanged = this.isRanged;
			m.isFearsome = this.isFearsome;
			m.isLarge = this.isLarge;
			m.isBloodThirsty = this.isBloodThirsty;
			m.isArmored = this.isArmored;
			m.isElite = this.isElite;
			m.defaultStats = this.defaultStats;
			m.isEasy = this.isEasy;
			m.isNormal = this.isNormal;
			m.isHard = this.isHard;
			m.cost = (int[])this.cost.Clone();
			m.moveSpecial = (string[])this.moveSpecial.Clone();
			m.tag = (string[])this.tag.Clone();
			m.special = (string[])this.special.Clone();
			m.isEmpty = this.isEmpty;
			m.triggerName = this.triggerName;
			m.negatedBy = this.negatedBy;
			m.monsterType = this.monsterType;
			m.modifierList = new ObservableCollection<MonsterModifier>(this.modifierList);
			return m;
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		public void LoadCustomModifiers(ObservableCollection<MonsterModifier> customModifiers)
        {
			//The default JSON converter for MonsterModifier can't look at the scenario's list of custom MonsterModifiers. So we need to hydrate it when we load the Monster in the MonsterEditorWindow.
			for(int i=0; i<modifierList.Count; i++)
            {
				if (modifierList[i].id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS)
				{
					MonsterModifier modData = customModifiers.First(it => it.id == modifierList[i].id);
					if (modData != null)
					{
                        modifierList[i] = modData;
					}
				}
            }
        }


		public static List<MonsterType> Goblins()
		{
			return new List<MonsterType> { MonsterType.GoblinScout, MonsterType.GoblinScout, MonsterType.VargRider };
		}

		public static List<MonsterType> Orcs()
		{
			return new List<MonsterType> { MonsterType.OrcHunter, MonsterType.OrcMarauder, MonsterType.OrcTaskmaster, MonsterType.HighOrcWarrior, MonsterType.Gargletarg, MonsterType.SupplicantOfMoreGoth, MonsterType.LordJavelin };
		}

		public static List<MonsterType> Humans()
		{
			return new List<MonsterType> { MonsterType.Ruffian, MonsterType.Soldier, MonsterType.Atari, MonsterType.Endris };
		}

		public static List<MonsterType> Spirits()
		{
			return new List<MonsterType> { MonsterType.Wight, MonsterType.Shadowman, MonsterType.Ursula, MonsterType.LichKing };
		}

		public static List<MonsterType> Trolls()
		{
			return new List<MonsterType> { MonsterType.CaveTroll, MonsterType.HillTroll, MonsterType.Oliver };
		}

		public static List<MonsterType> Vargs()
		{
			return new List<MonsterType> { MonsterType.Varg, MonsterType.VargRider, MonsterType.Chartooth };
		}

		public static List<MonsterType> Spiders()
		{
			return new List<MonsterType> { MonsterType.GiantSpider, MonsterType.SpawnOfUglygiant };
		}

		public static List<MonsterType> Flying()
		{
			return new List<MonsterType> { MonsterType.Balerock, MonsterType.FoulBeast, MonsterType.LichKing };
		}

		public static List<MonsterType> OtherBeasts()
		{
			return new List<MonsterType> { MonsterType.WarElephant, MonsterType.AnonymousThing };
		}

		public static List<MonsterType> AllBeasts()
		{
			List<MonsterType> monsterList = new List<MonsterType>();
			monsterList.AddRange(Trolls());
			monsterList.AddRange(Vargs());
			monsterList.AddRange(Spiders());
			monsterList.AddRange(Flying());
			monsterList.AddRange(OtherBeasts());
			return monsterList;
		}

		public static List<MonsterType> Humanoid()
		{
			List<MonsterType> monsterList = new List<MonsterType>();
			monsterList.AddRange(Goblins());
			monsterList.AddRange(Orcs());
			monsterList.AddRange(Humans());
			return monsterList;
		}
	}
}
