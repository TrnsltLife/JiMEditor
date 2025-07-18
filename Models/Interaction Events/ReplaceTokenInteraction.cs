﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JiME
{
	public class ReplaceTokenInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "replace-token";
			base.DefineTranslationAccessors();
		}

		//replace event
		string _eventToReplace, _replaceWithEvent;
		bool _noText;
		Guid _replaceWithGUID;

		public string eventToReplace
		{
			get => _eventToReplace;
			set
			{
				_eventToReplace = value;
				NotifyPropertyChanged( "eventToReplace" );
			}
		}
		public string replaceWithEvent
		{
			get => _replaceWithEvent;
			set
			{
				_replaceWithEvent = value;
				NotifyPropertyChanged( "replaceWithEvent" );
			}
		}
		public bool noText
		{
			get => _noText;
			set
			{
				_noText = value;
				NotifyPropertyChanged( "noText" );
			}
		}
		public Guid replaceWithGUID
		{
			get => _replaceWithGUID;
			set
			{
				_replaceWithGUID = value;
				NotifyPropertyChanged( "replaceWithGUID" );
			}
		}

		public ReplaceTokenInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Replace;

			eventToReplace = replaceWithEvent = "None";
			noText = true;
			replaceWithGUID = Guid.Empty;
		}

		public ReplaceTokenInteraction Clone()
		{
			ReplaceTokenInteraction interact = new ReplaceTokenInteraction("");
			base.CloneInto(interact);
			interact.eventToReplace = this.eventToReplace;
			interact.replaceWithEvent = this.replaceWithEvent;
			interact.noText = this.noText;
			interact.replaceWithGUID = this.replaceWithGUID;
			return interact;
		}

		new public List<string> CollectEvents()
		{
			List<string> events = base.CollectEvents();
			events.Add(eventToReplace);
			events.Add(replaceWithEvent);
			return events;
		}
	}
}
