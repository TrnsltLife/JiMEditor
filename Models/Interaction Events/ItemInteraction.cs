using System.ComponentModel;
using System.Collections.ObjectModel;

namespace JiME
{
	public class ItemInteraction : PersistentInteractionBase, INotifyPropertyChanged, ICommonData
	{
		string _finishedTrigger;
		int _randomizedItemsCount;

		public ObservableCollection<int> itemList { get; set; } = new ObservableCollection<int>();
		public string finishedTrigger
		{
			get { return _finishedTrigger; }
			set
			{
				_finishedTrigger = value;
				NotifyPropertyChanged("finishedTrigger");
			}
		}

		public int randomItemsCount
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
		}

		public ItemInteraction Clone()
		{
			ItemInteraction interact = new ItemInteraction("");
			base.CloneInto(interact);
			return interact;
		}
	}
}
