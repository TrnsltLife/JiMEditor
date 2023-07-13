using System.Collections.Generic;
using System.ComponentModel;

namespace JiME
{
	public class DecisionInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "decision";
			base.DefineTranslationAccessors();
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("event.{1}.{0}.choice1", () => this.choice1),
				new TranslationAccessor("event.{1}.{0}.choice2", () => this.choice2),
				new TranslationAccessor("event.{1}.{0}.choice3", () => this.isThreeChoices ? this.choice3 : "")
			};
			translationAccessors.AddRange(list);
		}

		//Decision
		string _c1t, _c2t, _c3t;
		public string choice1 { get; set; }
		public string choice2 { get; set; }
		public string choice3 { get; set; }
		public bool isThreeChoices { get; set; }
		public string choice1Trigger
		{
			get => _c1t;
			set
			{
				if ( _c1t != value )
				{
					_c1t = value;
					NotifyPropertyChanged( "choice1Trigger" );
				}
			}
		}
		public string choice2Trigger
		{
			get => _c2t;
			set
			{
				if ( _c2t != value )
				{
					_c2t = value;
					NotifyPropertyChanged( "choice2Trigger" );
				}
			}
		}
		public string choice3Trigger
		{
			get => _c3t;
			set
			{
				if ( _c3t != value )
				{
					_c3t = value;
					NotifyPropertyChanged( "choice3Trigger" );
				}
			}
		}

		public DecisionInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Decision;

			isThreeChoices = false;
			choice1 = "Choice One";
			choice2 = "Choice Two";
			choice3 = "Choice Three";
			choice1Trigger = choice2Trigger = choice3Trigger = "None";
		}

		public DecisionInteraction Clone()
		{
			DecisionInteraction interact = new DecisionInteraction("");
			base.CloneInto(interact);
			interact.choice1 = this.choice1;
			interact.choice2 = this.choice2;
			interact.choice3 = this.choice3;
			interact.choice1Trigger = this.choice1Trigger;
			interact.choice2Trigger = this.choice2Trigger;
			interact.choice3Trigger = this.choice3Trigger;
			interact.isThreeChoices = this.isThreeChoices;
			return interact;
		}

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			if ( choice1Trigger == oldName )
				choice1Trigger = newName;

			if ( choice2Trigger == oldName )
				choice2Trigger = newName;

			if ( choice3Trigger == oldName )
				choice3Trigger = newName;
		}
	}
}
