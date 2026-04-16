// Ported from: JiMEditor/Views/Interactions/TitleInteractionWindow.xaml — Title tab (2026-04-15).
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

public partial class TitleEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	TitleInteraction? _titleInteraction;
	bool _suppress;
	int _lastSliderMax;
	List<Title> _availableTitles = new();

	public TitleEventCustomTab()
	{
		InitializeComponent();
		TitleSelectCombo.SelectionChanged += TitleSelectCombo_OnSelectionChanged;
	}

	public void Attach( Scenario scenario, TitleInteraction titleInteraction )
	{
		Detach();
		_scenario = scenario;
		_titleInteraction = titleInteraction;
		_availableTitles = BuildTitleOptions( scenario );
		TitleSelectCombo.ItemsSource = _availableTitles;
		FallbackTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		SelectedTitlesItems.ItemsSource = titleInteraction.titleList;
		titleInteraction.titleList.CollectionChanged += TitleList_OnCollectionChanged;
	}

	static List<Title> BuildTitleOptions( Scenario scenario ) =>
		Titles.list
			.Where( t => t.id != 0 && scenario.collectionObserver.Contains( Collection.FromID( t.collection ) ) )
			.OrderBy( t => t.id )
			.ToList();

	public void Hydrate()
	{
		if ( _titleInteraction == null )
			return;
		TitleSelectCombo.SelectedItem = _availableTitles.FirstOrDefault();
		SelectTrigger( FallbackTriggerCombo, _titleInteraction.fallbackTrigger );
		LoreFallbackTextBox.Text = _titleInteraction.loreFallback.ToString( CultureInfo.InvariantCulture );
		XpFallbackTextBox.Text = _titleInteraction.xpFallback.ToString( CultureInfo.InvariantCulture );
		ThreatFallbackTextBox.Text = _titleInteraction.threatFallback.ToString( CultureInfo.InvariantCulture );
		RefreshSliderBounds();
	}

	internal void Commit()
	{
		if ( _titleInteraction == null )
			return;

		_titleInteraction.fallbackTrigger = FallbackTriggerCombo.SelectedItem is Trigger t ? t.dataName : "None";
		_titleInteraction.randomizedTitlesCount = (int)System.Math.Round( RandomizedTitlesSlider.Value );
		_titleInteraction.loreFallback = ParseIntOrZero( LoreFallbackTextBox.Text );
		_titleInteraction.xpFallback = ParseIntOrZero( XpFallbackTextBox.Text );
		_titleInteraction.threatFallback = ParseIntOrZero( ThreatFallbackTextBox.Text );
	}

	static int ParseIntOrZero( string? text ) =>
		int.TryParse( (text ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value ) ? value : 0;

	public void Detach()
	{
		if ( _titleInteraction != null )
			_titleInteraction.titleList.CollectionChanged -= TitleList_OnCollectionChanged;
		_titleInteraction = null;
		_scenario = null;
		_lastSliderMax = 0;
		_availableTitles = new List<Title>();
	}

	public Control AsControl() => this;

	void TitleList_OnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) => RefreshSliderBounds();

	void RefreshSliderBounds()
	{
		if ( _titleInteraction == null )
			return;

		var count = _titleInteraction.titleList.Count;
		var max = System.Math.Max( 1, count );
		var prevMax = _lastSliderMax;
		var required = _titleInteraction.randomizedTitlesCount;
		if ( required < 1 )
			required = 1;
		if ( required > max )
			required = max;
		if ( prevMax > 0 && max > prevMax && required == prevMax )
			required = max;

		_titleInteraction.randomizedTitlesCount = required;

		_suppress = true;
		try
		{
			RandomizedTitlesSlider.Minimum = 1;
			RandomizedTitlesSlider.Maximum = max;
			RandomizedTitlesSlider.TickFrequency = 1;
			var ticks = new AvaloniaList<double>();
			for ( var i = 1; i <= max; i++ )
				ticks.Add( i );
			RandomizedTitlesSlider.Ticks = ticks;
			RandomizedTitlesSlider.Value = required;
			RandomizedTitlesValueText.Text = required.ToString( CultureInfo.InvariantCulture );
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

	void TitleSelectCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e ) =>
		AddSelectedTitleButton.IsEnabled = TitleSelectCombo.SelectedItem is Title;

	void RandomizedTitlesSlider_OnValueChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _titleInteraction == null )
			return;
		var value = (int)System.Math.Round( RandomizedTitlesSlider.Value );
		_titleInteraction.randomizedTitlesCount = value;
		RandomizedTitlesValueText.Text = value.ToString( CultureInfo.InvariantCulture );
	}

	void AddSelectedTitle_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _titleInteraction == null || TitleSelectCombo.SelectedItem is not Title selectedTitle )
			return;
		if ( !_titleInteraction.titleList.Contains( selectedTitle ) )
			_titleInteraction.titleList.Add( selectedTitle );
	}

	void RemoveTitle_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _titleInteraction == null || sender is not Button button || button.CommandParameter is not Title title )
			return;
		if ( _titleInteraction.titleList.Contains( title ) )
			_titleInteraction.titleList.Remove( title );
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
