// Ported from: JiMEditor/Views/Interactions/PersistentInteractionWindow.xaml.cs (2026-04-14) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class PersistentInteractionWindow : BaseEventDialogWindow
{
	readonly PersistentTokenInteraction _persistent;
	PersistentEventCustomTab? _tab;

	public PersistentInteractionWindow( Scenario scenario )
		: this( scenario, new PersistentTokenInteraction( "New Persistent Event" ), showCancelButton: true )
	{
	}

	public PersistentInteractionWindow( Scenario scenario, PersistentTokenInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Persistent(), showCancelButton )
	{
		_persistent = interaction;
		Title = "Persistent Event Editor";
		_persistent.isTokenInteraction = true;
	}

	protected override Control CreateCustomTab()
	{
		_tab = new PersistentEventCustomTab();
		_tab.Attach( Scenario, _persistent );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();

	protected override bool TryValidateForOk()
	{
		if ( !base.TryValidateForOk() )
			return false;
		if ( _persistent.alternativeTextTrigger is null or "" or "None" )
		{
			MessageBox.Show(
				"You must set the Alternative Flavor Text Trigger on the Persistent Event tab.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}
		if ( _persistent.eventToActivate is null or "" or "None" )
		{
			MessageBox.Show(
				"You must set the Event to Activate on the Persistent Event tab.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}
		return true;
	}
}
