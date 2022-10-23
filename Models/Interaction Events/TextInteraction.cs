using System.ComponentModel;

namespace JiME
{
	public class TextInteraction : PersistentInteractionBase, INotifyPropertyChanged, ICommonData
	{
		public TextInteraction( string name ) : base( name )
		{
		}
	}
}
