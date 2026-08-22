// Ported from: JiMEditor/Views/Interactions/ConditionalInteractionWindow.xaml.cs (2026-04-14) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class ConditionalInteractionWindow : BaseEventDialogWindow
{
	readonly ConditionalInteraction _conditional;
	ConditionalEventCustomTab? _tab;

	public ConditionalInteractionWindow( Scenario scenario )
		: this( scenario, new ConditionalInteraction( "New Conditional Event" ), showCancelButton: true )
	{
	}

	public ConditionalInteractionWindow( Scenario scenario, ConditionalInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Conditional(), showCancelButton )
	{
		_conditional = interaction;
		Title = "Conditional Event Editor";
		_conditional.isTokenInteraction = false;
	}

	protected override Control CreateCustomTab()
	{
		_tab = new ConditionalEventCustomTab();
		_tab.Attach( Scenario, _conditional );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();

	protected override bool TryValidateForOk()
	{
		if ( !base.TryValidateForOk() )
			return false;
		if ( _conditional.finishedTrigger is null or "" or "None" )
		{
			MessageBox.Show(
				"You must set the ...Fire This Trigger... Trigger on the Conditional Event tab.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		return true;
	}
}
