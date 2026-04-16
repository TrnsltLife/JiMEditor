// Ported from: JiMEditor/Views/Interactions/CorruptionInteractionWindow.xaml — Corruption tab (2026-04-15).
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.Views;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class CorruptionEventCustomTab : UserControl, IEventCustomTab
{
	CorruptionInteraction? _corruption;

	public CorruptionEventCustomTab()
	{
		InitializeComponent();
		CorruptionTargetCombo.ItemsSource = Enum.GetValues( typeof( CorruptionTarget ) ).Cast<CorruptionTarget>().ToArray();
	}

	public void Attach( CorruptionInteraction corruption )
	{
		_corruption = corruption;
	}

	public void Hydrate()
	{
		if ( _corruption == null )
			return;
		SelectCorruptionAmount( _corruption.corruption );
		CorruptionTargetCombo.SelectedItem = _corruption.corruptionTarget;
		UpdateInstructionButtonEnabled();
	}

	internal void Commit()
	{
		if ( _corruption == null )
			return;
		if ( CorruptionAmountCombo.SelectedItem is ComboBoxItem amountItem
		     && int.TryParse( amountItem.Content?.ToString(), out var amount ) )
			_corruption.corruption = amount;
		if ( CorruptionTargetCombo.SelectedItem is CorruptionTarget target )
			_corruption.corruptionTarget = target;
	}

	public void Detach()
	{
		_corruption = null;
	}

	public Control AsControl() => this;

	void CorruptionTargetCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e ) => UpdateInstructionButtonEnabled();

	void UpdateInstructionButtonEnabled()
	{
		var selected = CorruptionTargetCombo.SelectedItem as CorruptionTarget?;
		EditInstructionsButton.IsEnabled =
			selected == CorruptionTarget.ONE_HERO
			|| selected == CorruptionTarget.MULTIPLE_HEROES
			|| selected == CorruptionTarget.ALL_HEROES;
	}

	void SelectCorruptionAmount( int value )
	{
		foreach ( var item in CorruptionAmountCombo.Items.OfType<ComboBoxItem>() )
		{
			if ( int.TryParse( item.Content?.ToString(), out var parsed ) && parsed == value )
			{
				CorruptionAmountCombo.SelectedItem = item;
				return;
			}
		}

		CorruptionAmountCombo.SelectedIndex = 3;
	}

	async void EditInstructionsButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _corruption == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;

		var initial = _corruption.instructionText ?? "";
		var dlg = new StoryTextEditWindow( initial, "Choose which heroes must gain corruption token(s)." );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
			_corruption.instructionText = dlg.StoryText;
	}
}
