// Ported from: JiMEditor/Views/Interactions/CorruptionInteractionWindow.xaml.cs (2026-04-15) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class CorruptionInteractionWindow : BaseEventDialogWindow
{
	readonly CorruptionInteraction _corruption;
	CorruptionEventCustomTab? _tab;

	public CorruptionInteractionWindow( Scenario scenario )
		: this( scenario, new CorruptionInteraction( "New Corruption Event" ), showCancelButton: true )
	{
	}

	public CorruptionInteractionWindow( Scenario scenario, CorruptionInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Corruption(), showCancelButton )
	{
		_corruption = interaction;
		Title = "Corruption Event Editor";
	}

	protected override Control CreateCustomTab()
	{
		_tab = new CorruptionEventCustomTab();
		_tab.Attach( _corruption );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
