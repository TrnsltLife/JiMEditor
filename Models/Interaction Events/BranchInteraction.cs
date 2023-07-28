using System.Collections.Generic;
using System.ComponentModel;

namespace JiME
{
	public class BranchInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "story-branch";
			base.DefineTranslationAccessors();
		}

		bool _branchTestEvent;

		//Story branch
		string _triggerTest; //a trigger name
		string _triggerIsSetTrigger; //a trigger name
		string _triggerNotSetTrigger; //a trigger name
		string _triggerIsSet; //an event name
		string _triggerNotSet; //an event name
		public string triggerIsSet
		{
			get => _triggerIsSet;
			set
			{
				_triggerIsSet = value;
				NotifyPropertyChanged("triggerIsSet");
			}
		}
		public string triggerNotSet
		{
			get => _triggerNotSet;
			set
			{
				_triggerNotSet = value;
				NotifyPropertyChanged("triggerNotSet");
			}
		}
		public string triggerTest
		{
			get => _triggerTest;
			set
			{
				_triggerTest = value;
				NotifyPropertyChanged("triggerTest");
			}
		}
		public string triggerIsSetTrigger 
		{ 
			get => _triggerIsSetTrigger; 
			set
            {
				_triggerIsSetTrigger = value;
				NotifyPropertyChanged("triggerIsSetTrigger");
            } 
		}
		public string triggerNotSetTrigger
		{
			get => _triggerNotSetTrigger;
			set
			{
				_triggerNotSetTrigger = value;
				NotifyPropertyChanged( "triggerNotSetTrigger" );
			}
		}
		public bool branchTestEvent
		{
			get { return _branchTestEvent; }
			set
			{
				_branchTestEvent = value;
				NotifyPropertyChanged( "branchTestEvent" );
			}
		}

		public BranchInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Branch;

			triggerTest = triggerIsSet = triggerNotSet = triggerIsSetTrigger = triggerNotSetTrigger = "None";
			branchTestEvent = true;
			//default blank event text so it activates silently
			eventBookData.pages = new List<string>();
			eventBookData.pages.Add( "" );
		}

		public BranchInteraction Clone()
        {
			BranchInteraction interact = new BranchInteraction("");
			base.CloneInto(interact);
			interact.branchTestEvent = this.branchTestEvent;
			interact.triggerTest = this.triggerTest;
			interact.triggerIsSet = this.triggerIsSet;
			interact.triggerNotSet = this.triggerNotSet;
			interact.triggerIsSetTrigger = this.triggerIsSetTrigger;
			interact.triggerNotSetTrigger = this.triggerNotSetTrigger;
			return interact;
        }

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			if ( triggerIsSetTrigger == oldName )
				triggerIsSetTrigger = newName;

			if ( triggerNotSetTrigger == oldName )
				triggerNotSetTrigger = newName;

			if ( triggerTest == oldName )
				triggerTest = newName;
		}
	}
}
