using System;
using System.ComponentModel;

namespace JiME
{
	public class StartInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		public static readonly string DEFAULT_NAME = "Starting Position";

		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "start";
			base.DefineTranslationAccessors();
		}

		public StartInteraction(string name) : base(DEFAULT_NAME)
		{
			interactionType = InteractionType.Start;
			isTokenInteraction = true;
			tokenType = TokenType.Start;
			personType = PersonType.None;
			terrainType = TerrainType.None;
		}

		public StartInteraction() : this(DEFAULT_NAME)
		{
		}

		public StartInteraction Clone()
		{
			StartInteraction interact = new StartInteraction();
			base.CloneInto(interact);
			return interact;
		}
	}
}
