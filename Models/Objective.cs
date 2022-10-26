﻿using System;
using System.ComponentModel;

namespace JiME
{
	public class Objective : INotifyPropertyChanged, ICommonData
	{
		string _dataName, _eventName, _triggerName, _objectiveReminder, _nextTrigger, _triggeredByName;
		bool _skipSummary;
		int _loreReward, _xpReward, _threatReward;

		public string dataName
		{
			get => _dataName;
			set
			{
				if ( value != _dataName )
				{
					_dataName = value;
					NotifyChange( "dataName" );
				}
			}
		}//description in editor
		public Guid GUID { get; set; }
		public bool isEmpty { get; set; }
		public string eventName
		{
			get => _eventName;
			set
			{
				if ( value != _eventName )
				{
					_eventName = value;
					NotifyChange( "eventName" );
				}
			}
		}
		public string triggerName
		{
			get => _triggerName;
			set
			{
				if ( value != _triggerName )
				{
					_triggerName = value;
					NotifyChange( "triggerName" );
				}
			}
		}
		public string triggeredByName
		{
			get => _triggeredByName;
			set
			{
				if ( value != _triggeredByName )
				{
					_triggeredByName = value;
					NotifyChange( "triggeredByName" );
				}
			}
		}
		public string objectiveReminder
		{
			get { return _objectiveReminder; }
			set
			{
				if ( _objectiveReminder != value )
				{
					_objectiveReminder = value;
					NotifyChange( "objectiveReminder" );
				}
			}
		}
		public bool skipSummary
		{
			get => _skipSummary;
			set
			{
				if ( _skipSummary != value )
				{
					_skipSummary = value;
					NotifyChange( "skipSummary" );
				}
			}
		}
		public string nextTrigger
		{
			get => _nextTrigger;
			set
			{
				if ( value != _nextTrigger )
				{
					_nextTrigger = value;
					NotifyChange( "nextTrigger" );
				}
			}
		}
		public TextBookData textBookData { get; set; }
		public int loreReward
		{
			get => _loreReward;
			set
			{
				_loreReward = value;
				NotifyChange( "loreReward" );
			}
		}
		public int xpReward
		{
			get => _xpReward;
			set
			{
				_xpReward = value;
				NotifyChange( "xpReward" );
			}
		}
		public int threatReward
		{
			get => _threatReward;
			set
			{
				_threatReward = value;
				NotifyChange( "threatReward" );
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Objective( string shortname )
		{
			GUID = Guid.NewGuid();
			dataName = shortname;
			eventName = "None";
			triggerName = "None";
			triggeredByName = "None";
			objectiveReminder = "Default Objective Reminder";
			textBookData = new TextBookData( "Default Name" );
			textBookData.pages.Add( "Default Objective Text\n\nEdit this text to provide a summary to players so they understand what they need to accomplish to complete this Objective." );
			nextTrigger = "None";
			loreReward = xpReward = threatReward = 0;
		}

		public static Objective EmptyObjective()
		{
			Objective obj = new Objective( "None" );
			obj.textBookData.pages[0] = "None";
			obj.objectiveReminder = "";
			obj.isEmpty = true;
			return obj;
		}

		public Objective Clone()
		{
			Objective obj = new Objective("Copy of " + this.dataName);
			obj.eventName = this.eventName;
			obj.triggerName = this.triggerName;
			obj.objectiveReminder = this.objectiveReminder;
			obj.nextTrigger = this.nextTrigger;
			obj.triggeredByName = this.triggeredByName;
			obj.skipSummary = this.skipSummary;
			obj.loreReward = this.loreReward;
			obj.xpReward = this.xpReward;
			obj.threatReward = this.threatReward;
			obj.GUID = Guid.NewGuid();
			obj.isEmpty = this.isEmpty;
			obj.textBookData = this.textBookData.Clone();
			return obj;
		}

		public void RenameTrigger( string oldName, string newName )
		{
			if ( triggerName == oldName )
				triggerName = newName;

			if ( nextTrigger == oldName )
				nextTrigger = newName;

			if ( triggeredByName == oldName )
				triggeredByName = newName;
		}

		void NotifyChange( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
