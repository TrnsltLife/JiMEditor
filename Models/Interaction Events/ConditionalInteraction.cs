using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class ConditionalInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "conditional";
			base.DefineTranslationAccessors();
		}

		string _finishedTrigger;
		int _triggersRequiredCount;

		public ObservableCollection<string> triggerList { get; set; }
		public string finishedTrigger
		{
			get { return _finishedTrigger; }
			set
			{
				_finishedTrigger = value;
				NotifyPropertyChanged( "finishedTrigger" );
			}
		}

		public int triggersRequiredCount
        {
			get { return _triggersRequiredCount; }
			set
            {
				_triggersRequiredCount = value;
				NotifyPropertyChanged("triggersRequiredCount");
            }
        }

		public ConditionalInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Conditional;

			triggerList = new ObservableCollection<string>();
			finishedTrigger = "None";
			triggersRequiredCount = 0;
		}

		public ConditionalInteraction Clone()
		{
			ConditionalInteraction interact = new ConditionalInteraction("");
			base.CloneInto(interact);
			interact.finishedTrigger = this.finishedTrigger;
			interact.triggerList = new ObservableCollection<string>();
			foreach( string trigger in this.triggerList )
            {
				interact.triggerList.Add(trigger);
            }
			interact.triggersRequiredCount = this.triggersRequiredCount;
			return interact;
		}

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			if ( finishedTrigger == oldName )
				finishedTrigger = newName;

			for ( int i = 0; i < triggerList.Count; i++ )
			{
				if ( triggerList[i] == oldName )
					triggerList[i] = newName;
			}
		}
	}
}
