using JiME.UserControls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;

namespace JiME
{
	public class PersistentTokenInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "persistent-token";
			base.DefineTranslationAccessors();
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("event.{1}.{0}.altText", () => this.alternativeBookData.pages[0].StartsWith("This text will be shown") ? "" : this.alternativeBookData.pages[0])
			};
			translationAccessors.AddRange(list);
		}

		string _eventToActivate, _alternativeTextTrigger;
		public TextBookData alternativeBookData { get; set; }
		public string eventToActivate
		{
			get { return _eventToActivate; }
			set { _eventToActivate = value; NotifyPropertyChanged( "eventToActivate" ); }
		}
		public string alternativeTextTrigger
		{
			get { return _alternativeTextTrigger; }
			set { _alternativeTextTrigger = value; NotifyPropertyChanged( "alternativeTextTrigger" ); }
		}

		public PersistentTokenInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Persistent;

			alternativeBookData = new TextBookData();
			alternativeBookData.pages.Add( "This text will be shown persistently every time a player inspects this Event's Token, but only after the 'Alternative Flavor Text Trigger' has been set." );
			alternativeTextTrigger = "None";
			eventToActivate = "None";
			isTokenInteraction = true;
			isReusable = false;
		}

		public PersistentTokenInteraction Clone()
		{
			PersistentTokenInteraction interact = new PersistentTokenInteraction("");
			base.CloneInto(interact);
			interact.eventToActivate = this.eventToActivate;
			interact.alternativeTextTrigger = this.alternativeTextTrigger;
			interact.alternativeBookData = this.alternativeBookData.Clone();
			return interact;
		}

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			if ( alternativeTextTrigger == oldName )
				alternativeTextTrigger = newName;
		}

		[JsonIgnore]
		public FlowDocument AlternativeFlowDocument
		{
			//Transform the text to display icons properly in a TextBlock
			get => RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(alternativeBookData.pages[0], "", "FontFamily=\"Segoe UI\" FontSize=\"12\"");
		}
	}
}
