using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JiME
{
	public class TitleInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		int _loreFallback, _xpFallback, _threatFallback;
		string _fallbackTrigger;
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
