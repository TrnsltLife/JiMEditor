using System.Windows.Documents;
using System;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Newtonsoft.Json;
using JiME.UserControls;

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
					_dataName = value.ToString();
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

        [JsonIgnore]
		public FlowDocument TextFlowDocument
        {
			//Transform the text to display icons properly in a TextBlock
			get => CreateFlowDocumentFromSimpleHtml(text);
        }

        [JsonIgnore]
		public FlowDocument EffectFlowDocument
        {
			//Transform the effect to display icons properly in a TextBlock
			get => CreateFlowDocumentFromSimpleHtml(effect);
        }

		private FlowDocument CreateFlowDocumentFromSimpleHtml(string html)
		{
			if (html == null) { return null; }
			return RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(html, "", "FontFamily=\"Segoe UI\" FontSize=\"12\"");
		}


		public void UpdateValid()
        {
			valid[0] = (damage[0] > 0 || fear[0] > 0);
			valid[1] = (damage[1] > 0 || fear[1] > 0);
			valid[2] = (damage[2] > 0 || fear[2] > 0);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public MonsterActivationItem() 
		{
			GUID = Guid.NewGuid();
		}

		public MonsterActivationItem(int id)
        {
			GUID = Guid.NewGuid();
			this.id = id;
			this.dataName = id.ToString();
			this.negate = Ability.None;
			this.text = "New attack text.";
			this.effect = "New after effect (optional).";
			this.valid = new bool[] { true, true, true };
			this.damage = new int[] { 1, 2, 3 };
			this.fear = new int[] { 1, 2, 3 };
        }

		public MonsterActivationItem(DefaultActivationItem act)
		{
			GUID = Guid.NewGuid(); 
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
