// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml.cs (2026-04-13) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

/// <summary>WPF-equivalent dialog for <see cref="DialogInteraction"/>; UI shell from <see cref="BaseEventDialogWindow"/>.</summary>
public sealed class DialogInteractionWindow : BaseEventDialogWindow
{
	readonly DialogInteraction _dialog;
	DialogEventCustomTab? _dialogTab;

	/// <summary>New dialog event (WPF: <c>new DialogInteractionWindow(scenario)</c>).</summary>
	public DialogInteractionWindow( Scenario scenario )
		: this( scenario, new DialogInteraction( "New Dialog Event" ), showCancelButton: true )
	{
	}

	/// <summary>Edit, duplicate, or new with explicit instance.</summary>
	public DialogInteractionWindow( Scenario scenario, DialogInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Dialog(), showCancelButton )
	{
		_dialog = interaction;
		Title = "Dialog Event Editor";
	}

	/// <summary>The model being edited (WPF parity name).</summary>
	public DialogInteraction interaction => _dialog;

	protected override Control CreateCustomTab()
	{
		_dialogTab = new DialogEventCustomTab();
		_dialogTab.Attach( Scenario, _dialog );
		return _dialogTab;
	}

	protected override void CommitCustomTabToModel() => _dialogTab?.Commit();
}
