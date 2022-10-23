using System.ComponentModel;

namespace JiME
{
	public abstract class PersistentInteractionBase : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		bool _isPersistent;
		string _persistentText;
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

		public PersistentInteractionBase( string name ) : base( name )
		{
			interactionType = InteractionType.Text;

			isPersistent = false;
			persistentText = "";
		}
	}
}
