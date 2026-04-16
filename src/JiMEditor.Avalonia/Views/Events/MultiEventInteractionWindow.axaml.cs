// Ported from: JiMEditor/Views/Interactions/MultiEventWindow.xaml.cs (2026-04-15) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class MultiEventInteractionWindow : BaseEventDialogWindow
{
	readonly MultiEventInteraction _multi;
	MultiEventCustomTab? _tab;

	public MultiEventInteractionWindow( Scenario scenario )
		: this( scenario, new MultiEventInteraction( "New Multi-Event" ), showCancelButton: true )
	{
	}

	public MultiEventInteractionWindow( Scenario scenario, MultiEventInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.MultiEvent(), showCancelButton )
	{
		_multi = interaction;
		Title = "Multi-Event Editor";
		Loaded += (_, _) => SetEventTextSectionEnabled( !_multi.isSilent );
	}

	protected override Control CreateCustomTab()
	{
		_tab = new MultiEventCustomTab();
		_tab.Attach( Scenario, _multi );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
