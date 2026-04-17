// Ported from: JiMEditor/Views/Interactions/DecisionInteractionWindow.xaml.cs (2026-04-16) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class DecisionInteractionWindow : BaseEventDialogWindow
{
	readonly DecisionInteraction _decision;
	DecisionEventCustomTab? _tab;

	public DecisionInteractionWindow( Scenario scenario )
		: this( scenario, new DecisionInteraction( "New Decision Event" ), showCancelButton: true )
	{
	}

	public DecisionInteractionWindow( Scenario scenario, DecisionInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Decision(), showCancelButton )
	{
		_decision = interaction;
		Title = "Decision Event Editor";
	}

	protected override Control CreateCustomTab()
	{
		_tab = new DecisionEventCustomTab();
		_tab.Attach( Scenario, _decision );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
