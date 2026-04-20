// Ported from: JiMEditor/Views/TileEditorWindow.xaml.cs (2026-04-20) — Avalonia journey fixed-layout first pass.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views;

public partial class TileEditorWindow : Window, INotifyPropertyChanged
{
	const double HexOriginX = 32d;
	const double HexOriginY = 27.7128128d;
	const double HexCellX = 48d;
	const double HexCellY = 55.4256256d / 2d;

	readonly Dictionary<BaseTile, TileVisual> _visuals = new();
	readonly List<TilePoolOption> _tilePoolOptions = new();
	bool _dragging;
	BaseTile? _dragTile;
	Point _dragClickOffset;
	BaseTile? _selected;
	bool _syncingComboSelection;

	public Chapter chapter { get; }
	public Scenario scenario { get; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public BaseTile? selected
	{
		get => _selected;
		set
		{
			if ( ReferenceEquals( _selected, value ) )
				return;
			_selected = value;
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( selected ) ) );
			UpdateSelectedUi();
			UpdateVisualSelection();
		}
	}

	public TileEditorWindow()
	{
		InitializeComponent();
		chapter = null!;
		scenario = null!;
	}

	public TileEditorWindow( Scenario s, Chapter? c = null )
		: this()
	{
		scenario = s;
		chapter = c ?? new Chapter( "New Block" );
		DataContext = this;

		scenario.RefilterGlobalTilePool();
		RefreshTilePoolOptions();
		InChapterCombo.ItemsSource = chapter.tileObserver;
		ExploredTriggerCombo.ItemsSource = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();

		foreach ( var t in chapter.tileObserver.OfType<BaseTile>() )
			EnsureVisual( t );
		RedrawAll();
		UseGraphicsCheck.IsChecked = scenario.useTileGraphics;
		TileCanvas.Focus();

		KeyDown += TileEditorWindow_OnKeyDown;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e ) => Close();

	void AddTileButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( TilePoolCombo.SelectedItem is not TilePoolOption sel )
			return;
		if ( chapter.tileObserver.Count >= 5 )
			return;
		var id = sel.Id;

		var tile = new HexTile( id );
		tile.useGraphic = UseGraphicsCheck.IsChecked ?? scenario.useTileGraphics;
		tile.ChangeColor( GetUnusedColor() );
		EnsurePathRoot( tile );
		chapter.AddTile( tile );
		scenario.globalTilePool.Remove( id );
		EnsureVisual( tile );
		RedrawAll();
		selected = tile;
		SyncComboSelection();
		RefreshTilePoolOptions();
	}

	void RemoveTileButton_Click( object? sender, RoutedEventArgs e ) => DeleteSelectedTile();

	void DeleteSelectedTile()
	{
		if ( selected == null )
			return;
		var ret = MessageBox.Show(
			"Are you sure you want to delete this Tile?\n\nALL ITS TOKENS WILL BE DELETED.",
			"Delete Tile",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;

		var tile = selected;
		visualRemove( tile );
		scenario.globalTilePool.Add( tile.idNumber );
		chapter.RemoveTile( tile );
		selected = chapter.tileObserver.OfType<BaseTile>().FirstOrDefault();
		SortGlobalTilePool();
		RedrawAll();
		SyncComboSelection();
		RefreshTilePoolOptions();
	}

	void SyncComboSelection()
	{
		_syncingComboSelection = true;
		try
		{
			InChapterCombo.SelectedItem = selected;
		}
		finally
		{
			_syncingComboSelection = false;
		}
	}

	void InChapterCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _syncingComboSelection )
			return;
		selected = InChapterCombo.SelectedItem as BaseTile;
		TileCanvas.Focus();
	}

	void SideARadio_OnChecked( object? sender, RoutedEventArgs e ) => SetSelectedSide( "A" );
	void SideBRadio_OnChecked( object? sender, RoutedEventArgs e ) => SetSelectedSide( "B" );

	void SetSelectedSide( string side )
	{
		if ( selected == null || selected.tileSide == side )
			return;
		selected.tileSide = side;
		EnsurePathRoot( selected );
		if ( _visuals.TryGetValue( selected, out var tv ) )
		{
			var hexGeo = BuildHexGeometries( selected );
			tv.Honeycomb.Data = hexGeo.Honeycomb;
			tv.Outline.Data = hexGeo.Outline;
			tv.HitFill = hexGeo.HitFill;
			ConfigureTileImage( selected, tv );
		}
		ApplyTileTransform( selected );
	}

	void UseGraphicsCheck_OnChanged( object? sender, RoutedEventArgs e )
	{
		var useGraphics = UseGraphicsCheck.IsChecked ?? false;
		scenario.useTileGraphics = useGraphics;
		foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
			tile.useGraphic = useGraphics;
		RedrawAll();
	}

	void ExploredTriggerCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( selected == null )
			return;
		if ( ExploredTriggerCombo.SelectedItem is Trigger t )
			selected.triggerName = t.dataName;
	}

	async void AddExploredTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( !tw.Accepted || selected == null )
			return;
		selected.triggerName = tw.EnteredTriggerName;
		ExploredTriggerCombo.ItemsSource = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();
		UpdateSelectedUi();
	}

	void StartingTileCheck_OnChecked( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		foreach ( var otherTile in chapter.tileObserver.OfType<BaseTile>() )
		{
			if ( otherTile.GUID == selected.GUID )
				continue;
			otherTile.isStartTile = false;
			var st = otherTile.tokenList.FirstOrDefault( it => it.tokenType == TokenType.Start );
			if ( st != null )
				otherTile.tokenList.Remove( st );
		}
		selected.isStartTile = true;
	}

	void StartingTileCheck_OnUnchecked( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var st = selected.tokenList.FirstOrDefault( it => it.tokenType == TokenType.Start );
		if ( st != null )
			selected.tokenList.Remove( st );
		selected.isStartTile = false;
	}

	async void EditTokenButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var tw = new TokenEditorWindow( selected, scenario );
		await tw.ShowDialog( this );
		UpdateSelectedUi();
	}

	void RecenterTileButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var centerX = 950d / 2d;
		var centerY = 750d / 2d;
		var snapX = Utils.dragSnapX.OrderBy( x => Math.Abs( x - centerX ) ).FirstOrDefault();
		var snapY = Utils.dragSnapY.OrderBy( y => Math.Abs( y - centerY ) ).FirstOrDefault();
		selected.position = new System.Windows.Vector( snapX, snapY );
		ApplyTileTransform( selected );
		SyncComboSelection();
	}

	async void TileGalleryButton_Click( object? sender, RoutedEventArgs e )
	{
		var gw = new GalleryWindow( scenario, chapter.tileObserver.Count );
		await gw.ShowDialog( this );
		if ( !gw.Accepted || gw.selectedData.Length == 0 )
			return;

		foreach ( var t in gw.selectedData )
		{
			if ( chapter.tileObserver.Count >= 5 )
				break;
			var tile = BaseTile.CreateTile( t.Item1 );
			if ( tile is not HexTile )
				continue;
			tile.useGraphic = UseGraphicsCheck.IsChecked ?? scenario.useTileGraphics;
			tile.ChangeColor( GetUnusedColor() );
			tile.tileSide = t.Item2;
			EnsurePathRoot( tile );
			chapter.AddTile( tile );
			scenario.globalTilePool.Remove( t.Item1 );
			EnsureVisual( tile );
			selected = tile;
		}

		RedrawAll();
		SyncComboSelection();
		RefreshTilePoolOptions();
	}

	void TileCanvas_OnPointerPressed( object? sender, PointerPressedEventArgs e )
	{
		if ( !e.GetCurrentPoint( TileCanvas ).Properties.IsLeftButtonPressed )
			return;
		TileCanvas.Focus();
		var tile = HitTestTileAt( e.GetPosition( TileCanvas ) );
		if ( tile == null )
		{
			selected = null;
			SyncComboSelection();
			return;
		}

		selected = tile;
		SyncComboSelection();
		_dragging = true;
		_dragTile = tile;
		var p = e.GetPosition( TileCanvas );
		_dragClickOffset = new Point( p.X - tile.position.X, p.Y - tile.position.Y );
		e.Pointer.Capture( TileCanvas );

		if ( e.ClickCount != 2 )
			return;
		_dragging = false;
		_dragTile = null;
		e.Pointer.Capture( null );
		EditTokenButton_Click( sender, new RoutedEventArgs() );
	}

	void TileCanvas_OnPointerMoved( object? sender, PointerEventArgs e )
	{
		if ( !_dragging || _dragTile == null )
			return;

		var p = e.GetPosition( TileCanvas );
		if ( p.X < 0 || p.Y < 0 || p.X > TileCanvas.Width || p.Y > TileCanvas.Height )
		{
			_dragging = false;
			_dragTile = null;
			e.Pointer.Capture( null );
			return;
		}
		var clickPoint = new System.Windows.Vector( p.X - _dragClickOffset.X, p.Y - _dragClickOffset.Y );
		var snappedX = Utils.dragSnapX.FirstOrDefault( snapx => Math.Abs( clickPoint.X - snapx ) <= Utils.tolerance );
		var snappedY = Utils.dragSnapY.FirstOrDefault( snapy => Math.Abs( clickPoint.Y - snapy ) <= Utils.tolerance );
		if ( Math.Abs( clickPoint.X - snappedX ) <= Utils.tolerance && Math.Abs( clickPoint.Y - snappedY ) <= Utils.tolerance )
		{
			_dragTile.position = new System.Windows.Vector( snappedX, snappedY );
			ApplyTileTransform( _dragTile );
		}
	}

	void TileCanvas_OnPointerReleased( object? sender, PointerReleasedEventArgs e )
	{
		_dragging = false;
		_dragTile = null;
		e.Pointer.Capture( null );
	}

	void TileCanvas_OnPointerWheelChanged( object? sender, PointerWheelEventArgs e )
	{
		if ( selected == null )
			return;
		if ( e.Delta.Y > 0 )
			selected.angle += -1 * selected.rotationAngle;
		else if ( e.Delta.Y < 0 )
			selected.angle += selected.rotationAngle;
		selected.angle %= 360;
		ApplyTileTransform( selected );
	}

	void TileEditorWindow_OnKeyDown( object? sender, KeyEventArgs e )
	{
		if ( selected == null )
			return;
		if ( e.Key == Key.PageUp )
		{
			selected.angle += -1 * selected.rotationAngle;
			selected.angle %= 360;
			ApplyTileTransform( selected );
			e.Handled = true;
		}
		else if ( e.Key == Key.PageDown )
		{
			selected.angle += selected.rotationAngle;
			selected.angle %= 360;
			ApplyTileTransform( selected );
			e.Handled = true;
		}
		else if ( e.Key == Key.Delete )
		{
			DeleteSelectedTile();
			e.Handled = true;
		}
		else if ( e.Key == Key.Up )
		{
			selected.position = new System.Windows.Vector( selected.position.X, selected.position.Y - HexCellY );
			ApplyTileTransform( selected );
			e.Handled = true;
		}
		else if ( e.Key == Key.Down )
		{
			selected.position = new System.Windows.Vector( selected.position.X, selected.position.Y + HexCellY );
			ApplyTileTransform( selected );
			e.Handled = true;
		}
		else if ( e.Key == Key.Left )
		{
			selected.position = new System.Windows.Vector( selected.position.X - HexCellX, selected.position.Y );
			ApplyTileTransform( selected );
			e.Handled = true;
		}
		else if ( e.Key == Key.Right )
		{
			selected.position = new System.Windows.Vector( selected.position.X + HexCellX, selected.position.Y );
			ApplyTileTransform( selected );
			e.Handled = true;
		}
	}

	int GetUnusedColor()
	{
		var used = chapter.tileObserver.OfType<BaseTile>().Select( x => x.color ).ToArray();
		for ( var i = 0; i < chapter.tileObserver.Count; i++ )
		{
			if ( i < used.Length && !used.Contains( i ) )
				return i;
		}
		return chapter.tileObserver.Count;
	}

	void RedrawAll()
	{
		TileCanvas.Children.Clear();
		DrawGrid();
		foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
		{
			EnsureVisual( tile );
			var tv = _visuals[tile];
			if ( !TileCanvas.Children.Contains( tv.Host ) )
				TileCanvas.Children.Add( tv.Host );
			var hexGeo = BuildHexGeometries( tile );
			tv.Honeycomb.Data = hexGeo.Honeycomb;
			tv.Outline.Data = hexGeo.Outline;
			tv.HitFill = hexGeo.HitFill;
			ConfigureTileImage( tile, tv );
			tv.Image.IsVisible = tile.useGraphic;
			tv.Honeycomb.IsVisible = !tile.useGraphic;
			tv.Outline.IsVisible = tile.useGraphic;
			tv.Honeycomb.Fill = GetTileFillBrush( tile.color );
			ApplyTileTransform( tile );
		}
		UpdateVisualSelection();
	}

	void DrawGrid()
	{
		for ( int x = 0; x < 950; x += 48 )
		{
			var l = new Line { StartPoint = new Point( x, 0 ), EndPoint = new Point( x, 750 ), Stroke = Brushes.Gray, StrokeThickness = 1, Opacity = 0.2 };
			TileCanvas.Children.Add( l );
		}
		for ( double y = 0; y < 750; y += HexCellY )
		{
			var l = new Line { StartPoint = new Point( 0, y ), EndPoint = new Point( 950, y ), Stroke = Brushes.Gray, StrokeThickness = 1, Opacity = 0.2 };
			TileCanvas.Children.Add( l );
		}
	}

	void EnsureVisual( BaseTile tile )
	{
		if ( _visuals.ContainsKey( tile ) )
			return;

		EnsurePathRoot( tile );
		var dims = GetTileDims( tile );
		var host = new Canvas
		{
			Width = dims.X,
			Height = dims.Y,
			ClipToBounds = true,
			DataContext = tile
		};
		var img = new Image
		{
			DataContext = tile
		};
		var hexGeo = BuildHexGeometries( tile );
		var honeycomb = new Path
		{
			Data = hexGeo.Honeycomb,
			Fill = GetTileFillBrush( tile.color ),
			Stroke = Brushes.White,
			StrokeThickness = 2,
			DataContext = tile
		};
		var outline = new Path
		{
			Data = hexGeo.Outline,
			Fill = Brushes.Transparent,
			Stroke = Brushes.White,
			StrokeThickness = 2,
			DataContext = tile,
			IsVisible = false
		};
		host.Children.Add( img );
		host.Children.Add( honeycomb );
		host.Children.Add( outline );
		_visuals[tile] = new TileVisual( host, img, honeycomb, outline, hexGeo.HitFill, dims.X, dims.Y );
		ConfigureTileImage( tile, _visuals[tile] );
	}

	void ConfigureTileImage( BaseTile tile, TileVisual tv )
	{
		var bmp = LoadTileBitmap( tile );
		tv.Image.Source = bmp;
		if ( bmp == null )
			return;

		var srcW = bmp.Size.Width;
		var srcH = bmp.Size.Height;
		if ( srcW <= 0 || srcH <= 0 )
			return;

		// Match WPF BaseTile.Update scale behavior: scale image so longest side fits tile dims.
		var scale = srcW > srcH ? tv.Width / srcW : tv.Height / srcH;
		tv.Image.Width = srcW * scale;
		tv.Image.Height = srcH * scale;
		Canvas.SetLeft( tv.Image, 0 );
		Canvas.SetTop( tv.Image, 0 );
	}

	void ApplyTileTransform( BaseTile tile )
	{
		if ( !_visuals.TryGetValue( tile, out var tv ) )
			return;

		var left = tile.position.X - HexOriginX;
		var top = tile.position.Y - HexOriginY;
		Canvas.SetLeft( tv.Host, left );
		Canvas.SetTop( tv.Host, top );

		var cx = tile.pathRoot.X * 32d + HexOriginX;
		var cy = tile.pathRoot.Y * HexOriginY + HexOriginY;
		var relX = tv.Width <= 0 ? 0.5 : cx / tv.Width;
		var relY = tv.Height <= 0 ? 0.5 : cy / tv.Height;
		relX = Math.Clamp( relX, 0d, 1d );
		relY = Math.Clamp( relY, 0d, 1d );

		tv.Host.RenderTransformOrigin = new RelativePoint( relX, relY, RelativeUnit.Relative );
		tv.Host.RenderTransform = new RotateTransform( tile.angle );
	}

	void UpdateVisualSelection()
	{
		foreach ( var kv in _visuals )
		{
			var isSel = kv.Key == selected;
			kv.Value.Honeycomb.Stroke = isSel ? Brushes.Red : Brushes.White;
			kv.Value.Outline.Stroke = isSel ? Brushes.Red : Brushes.White;
			if ( isSel )
			{
				TileCanvas.Children.Remove( kv.Value.Host );
				TileCanvas.Children.Add( kv.Value.Host );
			}
		}
	}

	void UpdateSelectedUi()
	{
		if ( selected == null )
		{
			SelectedTilePanel.IsVisible = false;
			SelectedTileNumberText.Text = "";
			SelectedTileCollectionIconText.Text = "";
			TokenCountText.Text = "Tokens in Tile: 0";
			EditTokenButton.IsEnabled = false;
			SideARadio.IsChecked = false;
			SideBRadio.IsChecked = false;
			ExploredTriggerCombo.SelectedItem = null;
			return;
		}

		SelectedTilePanel.IsVisible = true;
		SelectedTileNumberText.Text = selected.idNumber.ToString();
		SelectedTileCollectionIconText.Text = selected.collection.FontCharacter;
		TokenCountText.Text = $"Tokens in Tile: {selected.tokenList.Count}";
		EditTokenButton.IsEnabled = true;
		SideARadio.IsChecked = selected.tileSide == "A";
		SideBRadio.IsChecked = selected.tileSide == "B";
		if ( ExploredTriggerCombo.ItemsSource is System.Collections.IEnumerable src )
			ExploredTriggerCombo.SelectedItem = src.Cast<Trigger>().FirstOrDefault( t => t.dataName == selected.triggerName );
		StartingTileCheck.IsChecked = selected.isStartTile;
	}

	void SortGlobalTilePool()
	{
		var sorted = scenario.globalTilePool.OrderBy( n => n ).ToList();
		scenario.globalTilePool.Clear();
		foreach ( var n in sorted )
			scenario.globalTilePool.Add( n );
	}

	void RefreshTilePoolOptions()
	{
		scenario.RefilterGlobalTilePool();
		_tilePoolOptions.Clear();
		foreach ( var id in scenario.filteredGlobalTilePool )
			_tilePoolOptions.Add( new TilePoolOption( id, Collection.FromTileNumber( id ).FontCharacter ) );
		TilePoolCombo.ItemsSource = _tilePoolOptions;
		if ( _tilePoolOptions.Count > 0 )
			TilePoolCombo.SelectedIndex = 0;
	}

	void EnsurePathRoot( BaseTile tile )
	{
		if ( tile is not HexTile )
			return;
		var dict = tile.tileSide == "A" ? Utils.tileDictionary : Utils.tileDictionaryB;
		if ( !dict.TryGetValue( tile.idNumber, out var data ) )
			return;
		var hexPoints = BaseTileData.ExtractCoords( data.coords );
		if ( hexPoints.Length > 0 )
			tile.pathRoot = new System.Windows.Vector( hexPoints[0].X, hexPoints[0].Y );
	}

	System.Windows.Vector GetTileDims( BaseTile tile )
	{
		var dict = tile.tileSide == "A" ? Utils.tileDictionary : Utils.tileDictionaryB;
		if ( !dict.TryGetValue( tile.idNumber, out var data ) )
			return new System.Windows.Vector( 256, 256 );
		return new System.Windows.Vector( data.width, data.height );
	}

	Bitmap? LoadTileBitmap( BaseTile tile )
	{
		try
		{
			var uri = new Uri( $"avares://JiMEditor.Avalonia/Assets/Tiles{tile.tileSide}/{tile.idNumber}.png" );
			using var stream = AssetLoader.Open( uri );
			var bmp = new Bitmap( stream );
			return bmp;
		}
		catch
		{
			return null;
		}
	}

	void visualRemove( BaseTile tile )
	{
		if ( _visuals.TryGetValue( tile, out var tv ) )
		{
			TileCanvas.Children.Remove( tv.Host );
			_visuals.Remove( tile );
		}
	}

	static IBrush GetTileFillBrush( int colorIndex )
	{
		return colorIndex switch
		{
			0 => Brushes.SaddleBrown,
			1 => Brushes.DodgerBlue,
			2 => Brushes.SeaGreen,
			3 => Brushes.DarkMagenta,
			4 => Brushes.DarkOrange,
			_ => Brushes.SaddleBrown
		};
	}

	BaseTile? HitTestTileAt( Point canvasPoint )
	{
		foreach ( var child in TileCanvas.Children.Reverse() )
		{
			if ( child is not Canvas host || host.DataContext is not BaseTile tile )
				continue;
			if ( !_visuals.TryGetValue( tile, out var tv ) )
				continue;
			if ( HitTile( tile, tv, canvasPoint ) )
				return tile;
		}
		return null;
	}

	static bool HitTile( BaseTile tile, TileVisual tv, Point canvasPoint )
	{
		var left = tile.position.X - HexOriginX;
		var top = tile.position.Y - HexOriginY;
		var localX = canvasPoint.X - left;
		var localY = canvasPoint.Y - top;
		var cx = tile.pathRoot.X * 32d + HexOriginX;
		var cy = tile.pathRoot.Y * HexOriginY + HexOriginY;
		var radians = -tile.angle * Math.PI / 180d;
		var dx = localX - cx;
		var dy = localY - cy;
		var unrotated = new Point(
			cx + dx * Math.Cos( radians ) - dy * Math.Sin( radians ),
			cy + dx * Math.Sin( radians ) + dy * Math.Cos( radians ) );
		return tv.HitFill.FillContains( unrotated );
	}

	static HexGeometries BuildHexGeometries( BaseTile tile )
	{
		var dict = tile.tileSide == "A" ? Utils.tileDictionary : Utils.tileDictionaryB;
		if ( !dict.TryGetValue( tile.idNumber, out var data ) )
		{
			var empty = Geometry.Parse( "M 0 0" );
			return new HexGeometries( empty, empty, empty );
		}

		var hexCoords = BaseTileData.ExtractCoords( data.coords );
		var honeycomb = new StreamGeometry();
		using ( var ctx = honeycomb.Open() )
		{
			foreach ( var hc in hexCoords )
			{
				var centerX = hc.X * HexCellX + HexOriginX;
				var centerY = hc.Y * HexOriginY + HexOriginY;
				var p0 = HexCorner( centerX, centerY, 0 );
				ctx.BeginFigure( p0, true );
				for ( var i = 1; i < 6; i++ )
					ctx.LineTo( HexCorner( centerX, centerY, i ) );
				ctx.EndFigure( true );
			}
		}

		var edgeCounts = new Dictionary<string, (Point A, Point B, int Count)>();
		foreach ( var hc in hexCoords )
		{
			var centerX = hc.X * HexCellX + HexOriginX;
			var centerY = hc.Y * HexOriginY + HexOriginY;
			for ( var i = 0; i < 6; i++ )
			{
				var a = HexCorner( centerX, centerY, i );
				var b = HexCorner( centerX, centerY, ( i + 1 ) % 6 );
				var key = CanonicalEdgeKey( a, b );
				edgeCounts.TryGetValue( key, out var existing );
				edgeCounts[key] = ( a, b, existing.Count + 1 );
			}
		}

		var boundaryEdges = edgeCounts.Values.Where( e => e.Count == 1 ).ToList();
		var adjacency = new Dictionary<string, List<Point>>();
		foreach ( var e in boundaryEdges )
		{
			var ka = PointKey( e.A );
			var kb = PointKey( e.B );
			if ( !adjacency.ContainsKey( ka ) ) adjacency[ka] = new List<Point>();
			if ( !adjacency.ContainsKey( kb ) ) adjacency[kb] = new List<Point>();
			adjacency[ka].Add( e.B );
			adjacency[kb].Add( e.A );
		}

		var outline = new StreamGeometry();
		var used = new HashSet<string>();
		using ( var ctx = outline.Open() )
		{
			foreach ( var edge in boundaryEdges )
			{
				var seedKey = CanonicalEdgeKey( edge.A, edge.B );
				if ( used.Contains( seedKey ) )
					continue;
				var loop = new List<Point> { edge.A, edge.B };
				used.Add( seedKey );
				while ( true )
				{
					var current = loop[^1];
					var prev = loop[^2];
					var next = adjacency[PointKey( current )]
						.FirstOrDefault( p => PointKey( p ) != PointKey( prev ) && !used.Contains( CanonicalEdgeKey( current, p ) ) );
					if ( PointKey( next ) == PointKey( default ) )
						break;
					used.Add( CanonicalEdgeKey( current, next ) );
					if ( PointKey( next ) == PointKey( loop[0] ) )
						break;
					loop.Add( next );
				}
				ctx.BeginFigure( loop[0], false );
				for ( var i = 1; i < loop.Count; i++ )
					ctx.LineTo( loop[i] );
				ctx.EndFigure( true );
			}
		}
		return new HexGeometries( honeycomb, outline, honeycomb );
	}

	static Point HexCorner( double centerX, double centerY, int corner )
	{
		var angle = Math.PI / 3d * corner;
		return new Point( centerX + 32d * Math.Cos( angle ), centerY + 32d * Math.Sin( angle ) );
	}

	static string PointKey( Point p ) => $"{Math.Round( p.X, 3 )},{Math.Round( p.Y, 3 )}";

	static string CanonicalEdgeKey( Point a, Point b )
	{
		var ka = PointKey( a );
		var kb = PointKey( b );
		return string.CompareOrdinal( ka, kb ) <= 0 ? $"{ka}|{kb}" : $"{kb}|{ka}";
	}

	sealed class TileVisual( Canvas host, Image image, Path honeycomb, Path outline, Geometry hitFill, double width, double height )
	{
		public Canvas Host { get; } = host;
		public Image Image { get; } = image;
		public Path Honeycomb { get; } = honeycomb;
		public Path Outline { get; } = outline;
		public Geometry HitFill { get; set; } = hitFill;
		public double Width { get; } = width;
		public double Height { get; } = height;
	}
	readonly record struct HexGeometries( Geometry Honeycomb, Geometry Outline, Geometry HitFill );
	readonly record struct TilePoolOption( int Id, string CollectionIcon );
}
