using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JiME
{
	public class ItemInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		int _loreFallback, _xpFallback, _threatFallback;
		string _fallbackTrigger;
		int _randomizedItemsCount;

		ObservableCollection<Item> _itemList { get; set; } = new ObservableCollection<Item>();

		[JsonConverter(typeof(ItemListConverter))]
		public ObservableCollection<Item> itemList
		{
			get => _itemList;
			set
			{
				if (value != _itemList)
				{
					_itemList = value;
					NotifyPropertyChanged("itemList");
				}
			}
		}

		public string fallbackTrigger
		{
			get { return _fallbackTrigger; }
			set
			{
				_fallbackTrigger = value;
				NotifyPropertyChanged("fallbackTrigger");
			}
		}

		public int loreFallback
		{
			get => _loreFallback;
			set
			{
				_loreFallback = value;
				NotifyPropertyChanged("loreFallback");
			}
		}
		public int xpFallback
		{
			get => _xpFallback;
			set
			{
				_xpFallback = value;
				NotifyPropertyChanged("xpFallback");
			}
		}
		public int threatFallback
		{
			get => _threatFallback;
			set
			{
				_threatFallback = value;
				NotifyPropertyChanged("threatFallback");
			}
		}

		public int randomizedItemsCount
		{
			get { return _randomizedItemsCount; }
			set
			{
				_randomizedItemsCount = value;
				NotifyPropertyChanged("randomizedItemsCount");
			}
		}

		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "item";
			base.DefineTranslationAccessors();
		}

		public ItemInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Item;

		}

		public ItemInteraction Clone()
		{
			ItemInteraction interact = new ItemInteraction("");
			base.CloneInto(interact);
			interact.randomizedItemsCount = this.randomizedItemsCount;
			interact.itemList = new ObservableCollection<Item>(this.itemList);
			interact.fallbackTrigger = this.fallbackTrigger;
			return interact;
		}

		new public List<string> CollectTriggers()
		{
			List<string> triggers = base.CollectTriggers();
			triggers.Add(fallbackTrigger);
			return triggers;
		}
	}
}
