using System.Collections.ObjectModel;
using System.Linq;
using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

/// <summary>
/// Parity checks for <see cref="Translatable.ApplyTranslationUpdatesAfterEditorDialog"/> on interaction models,
/// matching the post-<c>ShowDialog</c> pattern used in <see cref="JiMEditor.Avalonia.EventDialog.EventEditorFactory"/>.
/// </summary>
public class EventEditorTranslationTailTests
{
	[Fact]
	public void TextInteractionTranslationTail_RenamesKeys_WhenEventDataNameChanges()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var interaction = new TextInteraction( "OldTextEvent" );
			interaction.textBookData.pages[0] = "Custom flavor for test";

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "event.text.OldTextEvent.flavorText", "Translated flavor", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = interaction.CaptureStartingValues();
			var originalPrefix = interaction.TranslationKeyPrefix();
			var originalKeyName = interaction.TranslationKeyName();

			interaction.dataName = "NewTextEvent";
			interaction.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			Assert.DoesNotContain( translation.translationItems, i => i.key == "event.text.OldTextEvent.flavorText" );
			Assert.Contains( translation.translationItems, i => i.key == "event.text.NewTextEvent.flavorText" );
		} );
	}

	[Fact]
	public void TextInteractionTranslationTail_DecertifiesWhenFlavorTextChanges()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var interaction = new TextInteraction( "TextEventA" );
			interaction.textBookData.pages[0] = "Original flavor";

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "event.text.TextEventA.flavorText", "Translated flavor", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = interaction.CaptureStartingValues();
			var originalPrefix = interaction.TranslationKeyPrefix();
			var originalKeyName = interaction.TranslationKeyName();

			interaction.textBookData.pages[0] = "Revised flavor";
			interaction.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			var item = translation.translationItems.Single( i => i.key == "event.text.TextEventA.flavorText" );
			Assert.False( item.translationOK );
		} );
	}

	[Fact]
	public void DialogInteractionTranslationTail_RenamesKeys_WhenEventDataNameChanges()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var interaction = new DialogInteraction( "OldDialogEvent" );

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "event.dialog.OldDialogEvent.choice1", "Translated choice 1", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = interaction.CaptureStartingValues();
			var originalPrefix = interaction.TranslationKeyPrefix();
			var originalKeyName = interaction.TranslationKeyName();

			interaction.dataName = "NewDialogEvent";
			interaction.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			Assert.DoesNotContain( translation.translationItems, i => i.key == "event.dialog.OldDialogEvent.choice1" );
			Assert.Contains( translation.translationItems, i => i.key == "event.dialog.NewDialogEvent.choice1" );
		} );
	}

	[Fact]
	public void DialogInteractionTranslationTail_DecertifiesWhenChoiceBodyTextChanges()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var interaction = new DialogInteraction( "DialogEventB" )
			{
				c1Text = "Original body"
			};

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "event.dialog.DialogEventB.text1", "Translated body", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = interaction.CaptureStartingValues();
			var originalPrefix = interaction.TranslationKeyPrefix();
			var originalKeyName = interaction.TranslationKeyName();

			interaction.c1Text = "Updated body";
			interaction.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			var item = translation.translationItems.Single( i => i.key == "event.dialog.DialogEventB.text1" );
			Assert.False( item.translationOK );
		} );
	}
}
