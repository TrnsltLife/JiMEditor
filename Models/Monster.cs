using JiME.Models;
using System;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class Monster : INotifyPropertyChanged, ICommonData
	{
		int Light { get { return 2; } }
		int Medium { get { return 3; } }
		int Heavy { get { return 4; } }

		string _dataName, /*_name,*/ _bonuses;
		int _id, _activationsId, _count, _health, _shieldValue, _sorceryValue, _moveA, _moveB, _groupLimit, _figureLimit, _damage, _loreReward, _movementValue, _maxMovementValue;
		bool _isRanged, _isFearsome, _isLarge, _isBloodThirsty, _isArmored, _isElite, _defaultStats, _isEasy, _isNormal, _isHard;
		int[] _cost;
		string[] _moveSpecial, _tag, _special;

		public Guid GUID { get; set; }

		public int id
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
				if ( isLarge )
					b += "Large, ";
				if ( isBloodThirsty )
					b += "BloodThirsty, ";
				if ( isArmored )
					b += "Armored, ";
				if ( string.IsNullOrEmpty( b ) )
					return "No Bonuses";
				else
					return b.Substring( 0, b.Length - 2 );
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
				if ( _isArmored || _isBloodThirsty || _isLarge )
					isElite = true;
				else
					isElite = false;
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

		public event PropertyChangedEventHandler PropertyChanged;

		public Monster() { }

		public Monster(int id)
		{
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
			return m;
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}
	}
}
