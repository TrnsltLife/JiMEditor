using JiME.Models;
using System;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class MonsterActivationItem : INotifyPropertyChanged, ICommonData
	{

		string _dataName;
		int _id;
		Ability _negate;
		bool[] _valid = new bool[] { false, false, false };
		int[] _damage = new int[] { 0, 0, 0 }, _fear = new int[] { 0, 0, 0 };
		string _text, _effect;

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

		public string text
		{
			get => _text;
			set
			{
				if (value != _text)
				{
					_text = value;
					NotifyPropertyChanged("text");
				}
			}
		}

		public string effect
		{
			get => _effect;
			set
			{
				if (value != _effect)
				{
					_effect = value;
					NotifyPropertyChanged("effect");
				}
			}
		}

		public Ability negate
		{
			get => _negate;
			set
			{
				if (value != _negate)
				{
					_negate = value;
					NotifyPropertyChanged("negate");
				}
			}
		}

		public bool isEmpty { get; set; }
		public string triggerName { get; set; }

		public bool[] valid
		{
			get => _valid;
			set
			{
				if (value != _valid)
				{
					_valid = value;
					NotifyPropertyChanged("valid");
				}
			}
		}

		public int[] damage
		{
			get => _damage;
			set
			{
				if (value != _damage)
				{
					_damage = value;
					NotifyPropertyChanged("damage");
				}
			}
		}

		public int[] fear
		{
			get => _fear;
			set
			{
				if (value != _fear)
				{
					_fear = value;
					NotifyPropertyChanged("fear");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public MonsterActivationItem() { }

		public MonsterActivationItem(DefaultActivationItem act)
		{
			id = act.id;
			dataName = act.id.ToString();
			negate = act.negate;
			valid = (bool[])act.valid.Clone();
			damage = (int[])act.damage.Clone();
			fear = (int[])act.fear.Clone();
			text = act.text;
			effect = act.effect;
		}

		public MonsterActivationItem Clone()
        {
			MonsterActivationItem item = new MonsterActivationItem();
			item.id = this.id;
			item.dataName = this.id.ToString();
			item.negate = this.negate;
			item.valid = (bool[])this.valid.Clone();
			item.damage = (int[])this.damage.Clone();
			item.fear = (int[])this.fear.Clone();
			item.text = this.text;
			item.effect = this.effect;
			return item;
        }

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}
	}
}
