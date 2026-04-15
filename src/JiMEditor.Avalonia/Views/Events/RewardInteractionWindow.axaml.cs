// Ported from: JiMEditor/Views/Interactions/RewardInteractionWindow.xaml.cs (2026-04-14) — Avalonia + BaseEventDialogWindow.
using JiMEditor.Avalonia.EventDialog;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

/// <summary>WPF-equivalent dialog for <see cref="RewardInteraction"/> using shared General Properties.</summary>
public sealed class RewardInteractionWindow : BaseEventDialogWindow
{
	/// <summary>New Reward event (WPF: <c>new RewardInteractionWindow(scenario)</c>).</summary>
	public RewardInteractionWindow( Scenario scenario )
		: this( scenario, new RewardInteraction( "New Reward Event" ), showCancelButton: true )
	{
	}

	/// <summary>Edit, duplicate, or new with explicit instance.</summary>
	public RewardInteractionWindow( Scenario scenario, RewardInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Reward(), showCancelButton )
	{
		Title = "Reward Event Editor";
	}
}
