// Ported from: JiMEditor/Views/Interactions/BranchInteractionWindow.xaml.cs (2026-04-18) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class BranchInteractionWindow : BaseEventDialogWindow
{
	readonly BranchInteraction _branch;
	BranchEventCustomTab? _tab;

	public BranchInteractionWindow( Scenario scenario )
		: this( scenario, new BranchInteraction( "New Branch Event" ), showCancelButton: true )
	{
	}

	public BranchInteractionWindow( Scenario scenario, BranchInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Branch(), showCancelButton )
	{
		_branch = interaction;
		Title = "Story Branch Event Editor";
	}

	public BranchInteraction interaction => _branch;

	protected override Control CreateCustomTab()
	{
		_tab = new BranchEventCustomTab();
		_tab.Attach( Scenario, _branch );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
