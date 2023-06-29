using System.ComponentModel;

namespace JiME
{
	public class DarknessInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "darkness";
			base.DefineTranslationAccessors();
		}

		public DarknessInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Darkness;
		}
	}
}
