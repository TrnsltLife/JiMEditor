// Ported from: JiMEditor/Views/Interactions/TextInteractionWindow.xaml.cs (2026-04-17) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class TextInteractionWindow : BaseEventDialogWindow
{
	readonly TextInteraction _text;
	TextEventPersistentTab? _persistentTab;

	public TextInteractionWindow( Scenario scenario )
		: this( scenario, new TextInteraction( "New Text Event" ), showCancelButton: true )
	{
	}

	public TextInteractionWindow( Scenario scenario, TextInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Text(), showCancelButton )
	{
		_text = interaction;
		Title = "Text Event Editor";
	}

	public TextInteraction interaction => _text;

	protected override Control CreateCustomTab()
	{
		_persistentTab = new TextEventPersistentTab();
		_persistentTab.Attach( Scenario, _text );
		return _persistentTab;
	}

	protected override void CommitCustomTabToModel() => _persistentTab?.Commit();
}
