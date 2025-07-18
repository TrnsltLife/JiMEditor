﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JiME
{
	public class MultiEventInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "multi-event";
			base.DefineTranslationAccessors();
		}

		bool _usingTriggers, _isSilent;
		public ObservableCollection<string> eventList { get; set; }
		public ObservableCollection<string> triggerList { get; set; }
		public bool usingTriggers
		{
			get { return _usingTriggers; }
			set
			{
				_usingTriggers = value;
				NotifyPropertyChanged( "usingTriggers" );
			}
		}
		public bool isSilent
		{
			get { return _isSilent; }
			set
			{
				_isSilent = value;
				NotifyPropertyChanged( "isSilent" );
			}
		}

		public MultiEventInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.MultiEvent;

			eventList = new ObservableCollection<string>();
			triggerList = new ObservableCollection<string>();
			usingTriggers = true;
			isSilent = true;
		}

		public MultiEventInteraction Clone()
		{
			MultiEventInteraction interact = new MultiEventInteraction("");
			base.CloneInto(interact);
			interact.usingTriggers = this.usingTriggers;
			interact.isSilent = this.isSilent;
			interact.eventList = new ObservableCollection<string>();
			foreach(var item in this.eventList)
            {
				interact.eventList.Add( item );
            }
			interact.triggerList = new ObservableCollection<string>();
			foreach (var item in this.triggerList)
			{
				interact.triggerList.Add(item);
			}
			return interact;
		}

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			for ( int i = 0; i < triggerList.Count; i++ )
			{
				if ( triggerList[i] == oldName )
					triggerList[i] = newName;
			}
		}

		new public List<string> CollectTriggers()
		{
			List<string> triggers = base.CollectTriggers();
			foreach (string trigger in triggerList)
			{
				triggers.Add(trigger);
			}
			return triggers;
		}

		new public List<string> CollectEvents()
		{
			List<string> events = base.CollectEvents();
			foreach (string anEvent in eventList)
			{
				events.Add(anEvent);
			}
			return events;
		}
	}
}
