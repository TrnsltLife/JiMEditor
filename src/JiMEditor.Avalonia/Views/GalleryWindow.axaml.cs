// Ported from: JiMEditor/Views/GalleryWindow.xaml.cs (2026-04-17) — Avalonia UI.
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class GalleryWindow : Window, INotifyPropertyChanged
{
	int _page;
	string _selectedTiles = "None";
	string _max = "";
	int _maxSelect;
	int _indexOffset;
	int _indexLimit;
	int _tileCount;
	GalleryTile[] _galleryTiles = Array.Empty<GalleryTile>();
	bool _side = true;

	public ObservableCollection<GalleryTile> tileObserver { get; } = new();

	public Scenario scenario { get; private set; } = null!;

	public int maxSelect
	{
		get => _maxSelect;
		set
		{
			_maxSelect = value;
			max = "";
		}
	}

	public int page
	{
		get => _page;
		set
		{
			_page = value;
			PropChanged( nameof( page ) );
		}
	}

	public string selectedTiles
	{
		get => _selectedTiles;
		set
		{
			_selectedTiles = value;
			PropChanged( nameof( selectedTiles ) );
		}
	}

	public string max
	{
		get => _max;
		set
		{
			_max = "Select up to " + maxSelect;
			PropChanged( nameof( max ) );
		}
	}

	public Tuple<int, string>[] selectedData { get; private set; } = Array.Empty<Tuple<int, string>>();

	/// <summary>True when the user added tiles via <c>Add Selected Tiles</c>.</summary>
	public bool Accepted { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	void PropChanged( string name ) =>
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

	public GalleryWindow()
	{
		InitializeComponent();
	}

	public GalleryWindow( Scenario s, int used, bool allowSideToggle = true )
		: this()
	{
		scenario = s;
		sideB.IsEnabled = allowSideToggle;

		int[] ids = Utils.LoadTiles().ToArray();
		if ( scenario.scenarioTypeJourney )
		{
			_indexOffset = 0;
			_indexLimit = 2;
			_tileCount = Utils.tileGalleryCount - _indexLimit;
			_galleryTiles = new GalleryTile[_tileCount];
		}
		else
		{
			_indexOffset = 42;
			_indexLimit = 0;
			_tileCount = 2;
			_galleryTiles = new GalleryTile[2];
			ids = new[] { 998, 999 };
		}

		for ( int i = 0; i < _tileCount; i++ )
		{
			_galleryTiles[i] = new GalleryTile();
			_galleryTiles[i].id = ids[i];
			_galleryTiles[i].source = LoadGalleryTileBitmap( _galleryTiles[i].id, "A" );
			_galleryTiles[i].enabled = _galleryTiles[i].enabled && scenario.IsCollectionEnabled( _galleryTiles[i].collection );
		}

		for ( int i = 0; i < 4 && i < _tileCount; i++ )
			tileObserver.Add( _galleryTiles[i] );

		page = 1;
		left.IsEnabled = false;
		selectedTiles = "None";
		maxSelect = 5 - used;

		DataContext = this;

		UpdateGallery();
	}

	void Left_Click( object? sender, RoutedEventArgs e )
	{
		page = Math.Max( page - 1, 1 );
		if ( page == 1 )
			left.IsEnabled = false;
		right.IsEnabled = true;
		UpdateGallery();
	}

	void Right_Click( object? sender, RoutedEventArgs e )
	{
		var maxPage = ( _tileCount + 4 ) / 4;
		page = Math.Min( page + 1, maxPage );
		if ( page == maxPage )
			right.IsEnabled = false;
		left.IsEnabled = true;
		UpdateGallery();
	}

	void UpdateGallery()
	{
		for ( int i = 0; i < _tileCount; i++ )
		{
			if ( !_galleryTiles[i].selected && _side )
			{
				_galleryTiles[i].source = LoadGalleryTileBitmap( _galleryTiles[i].id, "A" );
				_galleryTiles[i].side = "A";
			}
			else if ( !_galleryTiles[i].selected && !_side )
			{
				_galleryTiles[i].source = LoadGalleryTileBitmap( _galleryTiles[i].id, "B" );
				_galleryTiles[i].side = "B";
			}
		}

		tileObserver.Clear();
		int start = ( page - 1 ) * 4;
		for ( int i = start; i < Math.Min( start + 4, _tileCount ); i++ )
			tileObserver.Add( _galleryTiles[i] );

		UpdateEnabled();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e ) => Close();

	void TileButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button b || b.DataContext is not GalleryTile gt )
			return;

		if ( !gt.selected && selectedTiles.Split( ',' ).Length > 4 )
			return;

		gt.selected = !gt.selected;
		gt.side = _side ? "A" : "B";

		string[] selected = ( from t in _galleryTiles where t.selected select t.id + t.side + "," ).ToArray();
		if ( selected.Length > 0 )
		{
			selectedTiles = selected.Aggregate( ( acc, x ) => acc + x );
			selectedTiles = selectedTiles.Substring( 0, selectedTiles.Length - 1 );
		}
		else
			selectedTiles = "None";

		UpdateGallery();
	}

	void UpdateEnabled()
	{
		int n = ( from t in _galleryTiles where t.selected select t.id + t.side + "," ).Count();

		if ( maxSelect == n )
		{
			for ( int i = 0; i < _galleryTiles.Length; i++ )
			{
				if ( !_galleryTiles[i].selected )
					_galleryTiles[i].enabled = false;
			}
		}
		else
		{
			for ( int i = 0; i < _galleryTiles.Length; i++ )
			{
				if ( !_galleryTiles[i].selected )
					_galleryTiles[i].enabled = true;
			}
		}

		for ( int i = 0; i < _galleryTiles.Length; i++ )
		{
			if ( !_galleryTiles[i].selected && !scenario.globalTilePool.Contains( _galleryTiles[i].id ) )
				_galleryTiles[i].enabled = false;

			if ( !scenario.IsCollectionEnabled( _galleryTiles[i].collection ) )
				_galleryTiles[i].enabled = false;
		}
	}

	void SideB_Click( object? sender, RoutedEventArgs e )
	{
		_side = false;
		UpdateGallery();
	}

	void SideA_Click( object? sender, RoutedEventArgs e )
	{
		_side = true;
		UpdateGallery();
	}

	void AddTilesButton_Click( object? sender, RoutedEventArgs e )
	{
		selectedData = ( from gt in _galleryTiles where gt.selected select Tuple.Create( gt.id, gt.side ) ).ToArray();
		Accepted = true;
		Close();
	}

	static Bitmap? LoadGalleryTileBitmap( int tileId, string side )
	{
		try
		{
			var uri = new Uri( $"avares://JiMEditor.Avalonia/Assets/Tiles{side}/{tileId}.png" );
			using var stream = AssetLoader.Open( uri );
			return new Bitmap( stream );
		}
		catch
		{
			return null;
		}
	}
}
