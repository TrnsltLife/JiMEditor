using System.ComponentModel;

namespace JiME
{
	public abstract class PersistentInteractionBase : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		bool _isPersistent;
		string _persistentText;
		bool _hasActivated; //if true, the Interaction will never display the ->> Action button
		public bool isPersistent
		{
			get => _isPersistent;
			set
			{
				_isPersistent = value;
				NotifyPropertyChanged( "isPersistent" );
			}
		}
		public string persistentText
		{
			get => _persistentText;
			set
			{
				_persistentText = value;
				NotifyPropertyChanged( "persistentText" );
			}
		}
		public bool hasActivated
		{
			get => _hasActivated;
			set
			{
				_hasActivated = value;
				NotifyPropertyChanged("hasActivated");
			}
		}

		public PersistentInteractionBase( string name ) : base( name )
		{
			interactionType = InteractionType.Text;

			isPersistent = false;
			persistentText = "";
			hasActivated = false;
		}

		public void CloneInto(PersistentInteractionBase interact)
		{
			base.CloneInto( interact);
			interact.isPersistent = this.isPersistent;
			interact.persistentText = this.persistentText;
			interact.hasActivated = this.hasActivated;
		}

	}
}
