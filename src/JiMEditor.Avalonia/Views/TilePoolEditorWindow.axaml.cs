// Ported from: JiMEditor/Views/TilePoolEditorWindow.xaml.cs (2026-04-17) — Avalonia UI.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class TilePoolEditorWindow : Window, INotifyPropertyChanged
{
	HexTile? _selected;
	bool _closing;
	bool _suppressExploreTriggerSync;
	bool _startingTileSuppress;

	public HexTile? selected
	{
		get => _selected;
		set
		{
			if ( ReferenceEquals( _selected, value ) )
				return;
			_selected = value;
			PropChanged( nameof( selected ) );
		}
	}

	public Scenario scenario { get; set; } = null!;
	public Chapter chapter { get; set; } = null!;

	public event PropertyChangedEventHandler? PropertyChanged;

	void PropChanged( string name ) =>
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

	public TilePoolEditorWindow()
	{
		InitializeComponent();
	}

	public TilePoolEditorWindow( Scenario s, Chapter c )
		: this()
	{
		scenario = s;
		chapter = c;
		scenario.RefilterGlobalTilePool();
		DataContext = this;
		selected = null;

		ExploreTriggerCombo.ItemsSource = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();

		Loaded += OnLoaded;
		Closing += ( _, e ) =>
		{
			if ( !_closing && !TryClose() )
				e.Cancel = true;
		};
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		StartingTileCheck.IsVisible = chapter.isStart;
	}

	bool TryClose() => true;

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( TryClose() )
		{
			_closing = true;
			Close();
		}

	}

	void ToRandomButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( GlobalList.SelectedItem is not int item )
			return;
		AddTile( item );
	}

	void GlobalList_OnDoubleTapped( object? sender, TappedEventArgs e )
	{
		if ( GlobalList.SelectedItem is not int item )
			return;
		AddTile( item );
	}

	void AddTile( int id, string side = "A" )
	{
		if ( chapter.tileObserver.Count >= 5 )
			return;

		scenario.globalTilePool.Remove( id );
		GlobalList.SelectedIndex = -1;
		var hex = new HexTile( id, true ) { tileSide = side };
		chapter.AddTile( hex );
		selected = hex;
		RandomList.SelectedIndex = RandomList.Items.Count - 1;
		ShowExploreStatus();
	}

	void RemoveTile()
	{
		if ( selected == null )
			return;

		scenario.globalTilePool.Add( selected.idNumber );
		chapter.RemoveTile( selected );

		var sorted = scenario.globalTilePool.OrderBy( k => k ).ToList();
		scenario.globalTilePool.Clear();
		foreach ( var n in sorted )
			scenario.globalTilePool.Add( n );

		selected = null;
		RandomList.SelectedIndex = RandomList.Items.Count > 0 ? 0 : -1;
		if ( RandomList.SelectedItem is HexTile h )
		{
			selected = h;
			TokenEditButton.IsEnabled = true;
			AddFlavorButton.IsEnabled = true;
		}

		if ( RandomList.Items.Count == 0 )
		{
			selected = null;
			TokenEditButton.IsEnabled = false;
			AddFlavorButton.IsEnabled = false;
			SideA.IsEnabled = SideB.IsEnabled = SideRandom.IsEnabled = false;
			ExploreStatus.IsVisible = false;
		}
	}

	void ToPoolButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( RandomList.SelectedItem is not HexTile h )
			return;
		selected = h;
		RemoveTile();
	}

	async void RandomList_OnDoubleTapped( object? sender, TappedEventArgs e )
	{
		if ( selected == null )
			return;
		await OpenTokenEditorAsync();
	}

	void RandomList_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		selected = null;
		if ( RandomList.SelectedItem is HexTile h )
		{
			selected = h;
			SideA.IsEnabled = SideB.IsEnabled = SideRandom.IsEnabled = true;
			SideA.IsChecked = h.tileSide == "A";
			SideB.IsChecked = h.tileSide == "B";
			SideRandom.IsChecked = h.tileSide == "Random";
			TokenEditButton.IsEnabled = SideRandom.IsChecked != true;
			_startingTileSuppress = true;
			try
			{
				StartingTileCheck.IsChecked = h.isStartTile;
			}
			finally
			{
				_startingTileSuppress = false;
			}
			SyncExploreTriggerComboFromSelected();
			ShowExploreStatus();
		}
		else
		{
			SideA.IsEnabled = SideB.IsEnabled = SideRandom.IsEnabled = false;
			TokenEditButton.IsEnabled = false;
			AddFlavorButton.IsEnabled = false;
			ExploreStatus.IsVisible = false;
		}
	}

	void SyncExploreTriggerComboFromSelected()
	{
		if ( selected == null )
			return;
		_suppressExploreTriggerSync = true;
		try
		{
			var t = ExploreTriggerCombo.ItemsSource is IEnumerable<object> src
				? src.OfType<Trigger>().FirstOrDefault( x => x.dataName == selected.triggerName )
				: null;
			ExploreTriggerCombo.SelectedItem = t;
		}
		finally
		{
			_suppressExploreTriggerSync = false;
		}
	}

	void ExploreTriggerCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressExploreTriggerSync || selected == null )
			return;
		if ( ExploreTriggerCombo.SelectedItem is Trigger t )
			selected.triggerName = t.dataName;
	}

	async void AddExploredTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( tw.Accepted )
		{
			ExploreTriggerCombo.ItemsSource = scenario.triggersObserver
				.OrderByDescending( x => x.isEmpty )
				.ThenBy( x => x.dataName )
				.ToList();
			if ( selected != null )
			{
				selected.triggerName = tw.EnteredTriggerName;
				SyncExploreTriggerComboFromSelected();
			}
		}
	}

	async void TileGalleryButton_Click( object? sender, RoutedEventArgs e )
	{
		var gw = new GalleryWindow( scenario, chapter.tileObserver.Count, true );
		await gw.ShowDialog( this );
		if ( gw.Accepted && gw.selectedData.Length > 0 )
		{
			foreach ( var t in gw.selectedData )
				AddTile( t.Item1, t.Item2 );
		}
	}

	void ShowExploreStatus()
	{
		if ( selected == null )
			return;
		AddFlavorButton.IsEnabled = true;
		ExploreStatus.IsVisible = true;
		ExploreStatus.Text = string.IsNullOrEmpty( selected.flavorBookData.pages[0] )
			? "Exploration Text is Empty"
			: "Exploration Text is Set";
	}

	async void TokenEditButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		await OpenTokenEditorAsync();
	}

	async Task OpenTokenEditorAsync()
	{
		if ( selected == null )
			return;
		var w = new TokenEditorWindow( selected, scenario, true );
		await w.ShowDialog( this );
		ShowExploreStatus();
	}

	void SideA_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected != null )
			selected.tileSide = "A";
		TokenEditButton.IsEnabled = true;
	}

	void SideB_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected != null )
			selected.tileSide = "B";
		TokenEditButton.IsEnabled = true;
	}

	void SideRandom_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected != null )
			selected.tileSide = "Random";
		TokenEditButton.IsEnabled = false;
	}

	async void AddFlavorButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var originals = selected.CaptureStartingValues();
		var originalKeyName = selected.TranslationKeyName();
		var originalPrefix = selected.TranslationKeyPrefix();

		var sw = new StoryTextEditWindow( selected.flavorBookData.pages[0] );
		await sw.ShowDialog( this );
		if ( sw.Accepted )
			selected.flavorBookData.pages[0] = sw.StoryText;

		if ( string.IsNullOrEmpty( selected.flavorBookData.pages[0] ) )
			ExploreStatus.Text = "Exploration Text is Empty";
		else
			ExploreStatus.Text = "Exploration Text is Set";

		selected.UpdateKeysStartingWith( scenario.translationObserver, originalPrefix );
		selected.DecertifyChangedValues( scenario.translationObserver, originals, originalKeyName );
	}

	void StartingTile_OnChecked( object? sender, RoutedEventArgs e )
	{
		if ( _startingTileSuppress || selected == null )
			return;
		foreach ( ITile t in chapter.tileObserver )
		{
			if ( t is not BaseTile otherTile )
				continue;
			if ( otherTile.idNumber != selected.idNumber )
			{
				otherTile.isStartTile = false;
				var st = otherTile.tokenList.FirstOrDefault( it => it.tokenType == TokenType.Start );
				if ( st != null )
					otherTile.tokenList.Remove( st );
			}
		}

		selected.isStartTile = true;
	}

	void StartingTile_OnUnchecked( object? sender, RoutedEventArgs e )
	{
		if ( _startingTileSuppress || selected == null )
			return;
		var t = selected.tokenList.FirstOrDefault( it => it.tokenType == TokenType.Start );
		if ( t != null )
			selected.tokenList.Remove( t );
		selected.isStartTile = false;
	}
}
