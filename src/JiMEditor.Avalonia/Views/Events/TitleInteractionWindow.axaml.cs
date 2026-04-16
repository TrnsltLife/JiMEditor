// Ported from: JiMEditor/Views/Interactions/TitleInteractionWindow.xaml.cs (2026-04-15) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class TitleInteractionWindow : BaseEventDialogWindow
{
	readonly TitleInteraction _title;
	TitleEventCustomTab? _tab;

	public TitleInteractionWindow( Scenario scenario )
		: this( scenario, new TitleInteraction( "New Title Event" ), showCancelButton: true )
	{
	}

	public TitleInteractionWindow( Scenario scenario, TitleInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Title(), showCancelButton )
	{
		_title = interaction;
		Title = "Title Event Editor";
	}

	protected override Control CreateCustomTab()
	{
		_tab = new TitleEventCustomTab();
		_tab.Attach( Scenario, _title );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
