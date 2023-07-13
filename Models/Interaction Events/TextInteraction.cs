using System.ComponentModel;

namespace JiME
{
	public class TextInteraction : PersistentInteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "text";
			base.DefineTranslationAccessors();
		}

		public TextInteraction( string name ) : base( name )
		{
		}

		public TextInteraction Clone()
		{
			TextInteraction interact = new TextInteraction("");
			base.CloneInto(interact);
			return interact;
		}
	}
}
