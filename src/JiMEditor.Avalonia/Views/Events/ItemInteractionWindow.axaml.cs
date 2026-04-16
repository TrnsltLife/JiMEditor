// Ported from: JiMEditor/Views/Interactions/ItemInteractionWindow.xaml.cs (2026-04-15) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class ItemInteractionWindow : BaseEventDialogWindow
{
	readonly ItemInteraction _item;
	ItemEventCustomTab? _tab;

	public ItemInteractionWindow( Scenario scenario )
		: this( scenario, new ItemInteraction( "New Item Event" ), showCancelButton: true )
	{
	}

	public ItemInteractionWindow( Scenario scenario, ItemInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Item(), showCancelButton )
	{
		_item = interaction;
		Title = "Item Event Editor";
	}

	protected override Control CreateCustomTab()
	{
		_tab = new ItemEventCustomTab();
		_tab.Attach( Scenario, _item );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
