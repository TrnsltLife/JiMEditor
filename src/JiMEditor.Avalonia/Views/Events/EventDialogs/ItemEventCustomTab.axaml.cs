// Ported from: JiMEditor/Views/Interactions/ItemInteractionWindow.xaml — Item tab (2026-04-15).
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class ItemEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	ItemInteraction? _itemInteraction;
	bool _suppress;
	int _lastSliderMax;
	List<Item> _availableItems = new();

	public ItemEventCustomTab()
	{
		InitializeComponent();
		ItemSelectCombo.SelectionChanged += ItemSelectCombo_OnSelectionChanged;
	}

	public void Attach( Scenario scenario, ItemInteraction itemInteraction )
	{
		Detach();
		_scenario = scenario;
		_itemInteraction = itemInteraction;
		_availableItems = BuildItemOptions( scenario );
		ItemSelectCombo.ItemsSource = _availableItems;
		FallbackTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		SelectedItemsItems.ItemsSource = itemInteraction.itemList;
		itemInteraction.itemList.CollectionChanged += ItemList_OnCollectionChanged;
	}

	static List<Item> BuildItemOptions( Scenario scenario ) =>
		Items.list
			.Where( it => scenario.collectionObserver.Contains( Collection.FromID( it.collection ) ) )
			.Where( it => (it.slotId == Slot.TRINKET && it.tier <= 1) || (it.slotId == Slot.MOUNT && it.tier == 0) )
			.OrderBy( it => it.id )
			.ToList();

	public void Hydrate()
	{
		if ( _itemInteraction == null )
			return;
		ItemSelectCombo.SelectedItem = _availableItems.FirstOrDefault();
		SelectTrigger( FallbackTriggerCombo, _itemInteraction.fallbackTrigger );
		LoreFallbackTextBox.Text = _itemInteraction.loreFallback.ToString( CultureInfo.InvariantCulture );
		XpFallbackTextBox.Text = _itemInteraction.xpFallback.ToString( CultureInfo.InvariantCulture );
		ThreatFallbackTextBox.Text = _itemInteraction.threatFallback.ToString( CultureInfo.InvariantCulture );
		RefreshSliderBounds();
	}

	internal void Commit()
	{
		if ( _itemInteraction == null )
			return;

		_itemInteraction.fallbackTrigger = FallbackTriggerCombo.SelectedItem is Trigger t ? t.dataName : "None";
		_itemInteraction.randomizedItemsCount = (int)System.Math.Round( RandomizedItemsSlider.Value );
		_itemInteraction.loreFallback = ParseIntOrZero( LoreFallbackTextBox.Text );
		_itemInteraction.xpFallback = ParseIntOrZero( XpFallbackTextBox.Text );
		_itemInteraction.threatFallback = ParseIntOrZero( ThreatFallbackTextBox.Text );
	}

	static int ParseIntOrZero( string? text ) =>
		int.TryParse( (text ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value ) ? value : 0;

	public void Detach()
	{
		if ( _itemInteraction != null )
			_itemInteraction.itemList.CollectionChanged -= ItemList_OnCollectionChanged;
		_itemInteraction = null;
		_scenario = null;
		_lastSliderMax = 0;
		_availableItems = new List<Item>();
	}

	public Control AsControl() => this;

	void ItemList_OnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) => RefreshSliderBounds();

	void RefreshSliderBounds()
	{
		if ( _itemInteraction == null )
			return;

		var count = _itemInteraction.itemList.Count;
		var max = System.Math.Max( 1, count );
		var prevMax = _lastSliderMax;
		var required = _itemInteraction.randomizedItemsCount;
		if ( required < 1 )
			required = 1;
		if ( required > max )
			required = max;
		if ( prevMax > 0 && max > prevMax && required == prevMax )
			required = max;

		_itemInteraction.randomizedItemsCount = required;

		_suppress = true;
		try
		{
			RandomizedItemsSlider.Minimum = 1;
			RandomizedItemsSlider.Maximum = max;
			RandomizedItemsSlider.TickFrequency = 1;
			var ticks = new AvaloniaList<double>();
			for ( var i = 1; i <= max; i++ )
				ticks.Add( i );
			RandomizedItemsSlider.Ticks = ticks;
			RandomizedItemsSlider.Value = required;
			RandomizedItemsValueText.Text = required.ToString( CultureInfo.InvariantCulture );
		}
		finally
		{
			_suppress = false;
		}

		_lastSliderMax = max;
	}

	static void SelectTrigger( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not System.Collections.IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}
		var list = src.Cast<Trigger>().ToList();
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName ) ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	void ItemSelectCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e ) =>
		AddSelectedItemButton.IsEnabled = ItemSelectCombo.SelectedItem is Item;

	void RandomizedItemsSlider_OnValueChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _itemInteraction == null )
			return;
		var value = (int)System.Math.Round( RandomizedItemsSlider.Value );
		_itemInteraction.randomizedItemsCount = value;
		RandomizedItemsValueText.Text = value.ToString( CultureInfo.InvariantCulture );
	}

	void AddSelectedItem_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _itemInteraction == null || ItemSelectCombo.SelectedItem is not Item selectedItem )
			return;
		if ( !_itemInteraction.itemList.Contains( selectedItem ) )
			_itemInteraction.itemList.Add( selectedItem );
	}

	void RemoveItem_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _itemInteraction == null || sender is not Button button || button.CommandParameter is not Item item )
			return;
		if ( _itemInteraction.itemList.Contains( item ) )
			_itemInteraction.itemList.Remove( item );
	}

	async void AddFallbackTrigger_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;
		FallbackTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
		SelectTrigger( FallbackTriggerCombo, tw.EnteredTriggerName );
	}
}
