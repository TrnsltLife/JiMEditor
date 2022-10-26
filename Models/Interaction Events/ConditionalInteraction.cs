using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class ConditionalInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		string _finishedTrigger;

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

		public ConditionalInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Conditional;

			triggerList = new ObservableCollection<string>();
			finishedTrigger = "None";
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
