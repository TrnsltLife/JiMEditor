using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace JiME
{
	public class TitleInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		string _finishedTrigger;
		int _randomizedTitlesCount;

		ObservableCollection<Title> _titleList { get; set; } = new ObservableCollection<Title>();

		[JsonConverter(typeof(TitleListConverter))]
		public ObservableCollection<Title> titleList
		{
			get => _titleList;
			set
			{
				if (value != _titleList)
				{
					_titleList = value;
					NotifyPropertyChanged("titleList");
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

		public int randomizedTitlesCount
		{
			get { return _randomizedTitlesCount; }
			set
			{
				_randomizedTitlesCount = value;
				NotifyPropertyChanged("randomizedTitlesCount");
			}
		}

		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "title";
			base.DefineTranslationAccessors();
		}

		public TitleInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Title;

		}

		public TitleInteraction Clone()
		{
			TitleInteraction interact = new TitleInteraction("");
			base.CloneInto(interact);
			interact.randomizedTitlesCount = this.randomizedTitlesCount;
			interact.titleList = new ObservableCollection<Title>(this.titleList);
			interact.finishedTrigger = this.finishedTrigger;
			return interact;
		}
	}
}
