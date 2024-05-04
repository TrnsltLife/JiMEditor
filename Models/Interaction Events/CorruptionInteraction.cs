using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace JiME
{
	public class CorruptionInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		int _corruption;
		CorruptionTarget _corruptionTarget;

		public int corruption
		{
			get => _corruption;
			set
			{
				_corruption = value;
				NotifyPropertyChanged("corruption");
			}
		}
		public CorruptionTarget corruptionTarget
		{
			get => _corruptionTarget;
			set
			{
				_corruptionTarget = value;
				NotifyPropertyChanged("corruptionTarget");
			}
		}

		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "corruption";
			base.DefineTranslationAccessors();
		}

		public CorruptionInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Corruption;
			corruption = 1;
			corruptionTarget = CorruptionTarget.ONE_HERO;

		}

		public CorruptionInteraction Clone()
		{
			CorruptionInteraction interact = new CorruptionInteraction("");
			base.CloneInto(interact);
			interact.corruption = this.corruption;
			interact.corruptionTarget = this.corruptionTarget;
			return interact;
		}
	}
}
