using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace JiME
{
	public class ItemInteraction : PersistentInteractionBase, INotifyPropertyChanged, ICommonData
	{
		string _finishedTrigger;
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

		public string finishedTrigger
		{
			get { return _finishedTrigger; }
			set
			{
				_finishedTrigger = value;
				NotifyPropertyChanged("finishedTrigger");
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
			interact.finishedTrigger = this.finishedTrigger;
			return interact;
		}
	}
}
