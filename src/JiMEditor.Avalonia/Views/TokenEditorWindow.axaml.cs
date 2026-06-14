// Ported from: JiMEditor/Views/TokenEditorWindow.xaml.cs (2026-04-20) — Avalonia first-pass functional editor.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using JiMEditor.Avalonia.EventDialog;
using JiME;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace JiMEditor.Avalonia.Views;

public partial class TokenEditorWindow : Window, INotifyPropertyChanged
{
	static readonly Regex s_groupSuffix = new( @"\sGRP\d+$", RegexOptions.Compiled );
	readonly Dictionary<Token, Shape> _tokenShapes = new();
	readonly Dictionary<string, IBrush> _imageBrushCache = new();
	Token? _selected;
	Token? _dragToken;
	bool _dragging;
	Point _dragClickOffset;
	System.Windows.Vector _lastPos;

	public Scenario scenario { get; }
	public BaseTile tile { get; }
	public ObservableCollection<IInteraction> tokenInteractions { get; } = new();
	public event PropertyChangedEventHandler? PropertyChanged;

	public Token? selected
	{
		get => _selected;
		set
		{
			if ( ReferenceEquals( _selected, value ) )
				return;
			_selected = value;
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( selected ) ) );
		}
	}

	public TokenEditorWindow()
	{
		InitializeComponent();
		scenario = null!;
		tile = null!;
	}

	public TokenEditorWindow( BaseTile bt, Scenario s, bool fromRandom = false, Token? highlightToken = null )
		: this()
	{
		scenario = s;
		tile = bt;
		DataContext = this;

		RefreshTokenInteractions();
		InteractionCombo.ItemsSource = tokenInteractions;
		TriggeredByCombo.ItemsSource = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();
		scenario.interactionObserver.CollectionChanged += InteractionsCollectionChanged;
		KeyDown += TokenEditorWindow_OnKeyDown;
		TileImage.Source = LoadTileBitmap();

		if ( tile.tokenList.Count > 0 )
		{
			selected = highlightToken != null
				? tile.tokenList.FirstOrDefault( t => t.GUID == highlightToken.GUID ) ?? tile.tokenList[0]
				: tile.tokenList[0];
			TokenList.SelectedItem = selected;
		}

		ExplorationPanel.IsVisible = !fromRandom && tile.tileType == TileType.Hex;
		RebuildTokenCanvas();
		UpdateExploreStatus();
		UpdateSelectedUi();
		UpdateCapacityUi();
	}

	void InteractionsCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		RefreshTokenInteractions();
		if ( selected == null )
			return;
		InteractionCombo.SelectedItem = tokenInteractions.FirstOrDefault( x => x.dataName == selected.triggerName );
	}

	void RefreshTokenInteractions()
	{
		var previous = InteractionCombo.SelectedItem as IInteraction;
		tokenInteractions.Clear();
		foreach ( var interaction in scenario.interactionObserver.Where( IsAllowedTokenInteraction ) )
			tokenInteractions.Add( interaction );
		var restore = previous != null
			? tokenInteractions.FirstOrDefault( x => x.dataName == previous.dataName )
			: null;
		InteractionCombo.SelectedItem = restore;
	}

	bool IsAllowedTokenInteraction( IInteraction x ) =>
		( x.isTokenInteraction && !s_groupSuffix.IsMatch( x.dataName ) && x.dataName != StartInteraction.DEFAULT_NAME )
		|| x.dataName == "None"
		|| ( tile.isStartTile && x.dataName == StartInteraction.DEFAULT_NAME );

	bool CanAddMoreTokens() =>
		scenario.ignoreTokenLimits || tile.tokenList.Count < tile.tokenCount;

	void UpdateCapacityUi()
	{
		var used = tile.tokenList.Count;
		var max = tile.tokenCount;
		CapacityText.Inlines.Clear();
		CapacityText.Inlines.Add( new Run( "Token Slots Used: " ) );
		var usedRun = new Run( used.ToString() );
		if ( used > max )
			usedRun.Foreground = Brushes.Red;
		CapacityText.Inlines.Add( usedRun );
		CapacityText.Inlines.Add( new Run( $" / {max}" ) );
		AddTokenButton.IsEnabled = CanAddMoreTokens();
		RemoveTokenButton.IsEnabled = selected != null;
	}

	void UpdateSelectedUi()
	{
		if ( selected == null )
		{
			SelectedTokenText.Text = "No token selected.";
			InteractionCombo.SelectedItem = null;
			TriggeredByCombo.SelectedItem = null;
			return;
		}

		SelectedTokenText.Text = $"{selected.dataName} ({selected.tokenType})";
		InteractionCombo.SelectedItem = tokenInteractions.FirstOrDefault( x => x.dataName == selected.triggerName );
		if ( TriggeredByCombo.ItemsSource is System.Collections.IEnumerable src )
			TriggeredByCombo.SelectedItem = src.Cast<Trigger>().FirstOrDefault( t => t.dataName == selected.triggeredByName );
		HighlightSelection();
	}

	void UpdateExploreStatus()
	{
		if ( tile is HexTile hex )
		{
			ExploreStatusText.Text = string.IsNullOrEmpty( hex.flavorBookData.pages[0] )
				? "Exploration Text is Empty"
				: "Exploration Text is Set";
		}
		else
		{
			ExploreStatusText.Text = "Exploration Text: N/A";
		}
	}

	void TokenList_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		selected = TokenList.SelectedItem as Token;
		UpdateSelectedUi();
		UpdateCapacityUi();
	}

	void AddTokenButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( !CanAddMoreTokens() )
			return;
		var t = new Token( TokenType.None );
		tile.tokenList.Add( t );
		selected = t;
		TokenList.SelectedItem = selected;
		RebuildTokenCanvas();
		UpdateSelectedUi();
		UpdateCapacityUi();
	}

	void RemoveTokenButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var idx = tile.tokenList.IndexOf( selected );
		tile.tokenList.Remove( selected );
		selected = null;
		if ( tile.tokenList.Count > 0 )
		{
			var next = Math.Min( idx, tile.tokenList.Count - 1 );
			selected = tile.tokenList[next];
		}
		TokenList.SelectedItem = selected;
		RebuildTokenCanvas();
		UpdateSelectedUi();
		UpdateCapacityUi();
	}

	void InteractionCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( selected == null || InteractionCombo.SelectedItem is not IInteraction i )
			return;
		selected.triggerName = i.dataName;
		selected.tokenType = i.tokenType;
		selected.personType = i.personType;
		selected.terrainType = i.terrainType;
		RebuildTokenCanvas();
		UpdateSelectedUi();
	}

	void TriggeredByCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( selected == null || TriggeredByCombo.SelectedItem is not Trigger t )
			return;
		selected.triggeredByName = t.dataName;
	}

	async void AddInteractionButton_Click( object? sender, RoutedEventArgs e )
	{
		var flyout = new MenuFlyout();
		AddEventMenuItem( flyout, "Dialog Event", EventEditorFactory.TryCreateDialogInteractionAsync );
		AddEventMenuItem( flyout, "Text Event", EventEditorFactory.TryCreateTextInteractionAsync );
		AddEventMenuItem( flyout, "Reward Event", EventEditorFactory.TryCreateRewardInteractionAsync );
		AddEventMenuItem( flyout, "Persistent Event", EventEditorFactory.TryCreatePersistentInteractionAsync );
		AddEventMenuItem( flyout, "Conditional Event", EventEditorFactory.TryCreateConditionalInteractionAsync );
		AddEventMenuItem( flyout, "Item Event", EventEditorFactory.TryCreateItemInteractionAsync );
		AddEventMenuItem( flyout, "Title Event", EventEditorFactory.TryCreateTitleInteractionAsync );
		AddEventMenuItem( flyout, "Corruption Event", EventEditorFactory.TryCreateCorruptionInteractionAsync );
		AddEventMenuItem( flyout, "Decision Event", EventEditorFactory.TryCreateDecisionInteractionAsync );
		AddEventMenuItem( flyout, "Multi-Event", EventEditorFactory.TryCreateMultiEventInteractionAsync );
		AddEventMenuItem( flyout, "Enemy Event", EventEditorFactory.TryCreateThreatInteractionAsync );
		AddEventMenuItem( flyout, "Story Branch", EventEditorFactory.TryCreateBranchInteractionAsync );
		AddEventMenuItem( flyout, "Stat Test Event", EventEditorFactory.TryCreateStatTestInteractionAsync );
		flyout.ShowAt( AddInteractionButton );
		await System.Threading.Tasks.Task.CompletedTask;
	}

	void AddEventMenuItem<TInteraction>(
		MenuFlyout flyout,
		string title,
		Func<Window, Scenario, System.Threading.Tasks.Task<(bool accepted, TInteraction created)>> factory )
		where TInteraction : IInteraction
	{
		var item = new MenuItem { Header = title };
		item.Click += async (_, _) =>
		{
			var ( accepted, created ) = await factory( this, scenario );
			if ( !accepted )
				return;
			scenario.AddInteraction( created );
			RefreshTokenInteractions();
			InteractionCombo.SelectedItem = tokenInteractions.FirstOrDefault( x => x.dataName == created.dataName );
			if ( selected == null )
				return;
			selected.triggerName = created.dataName;
			selected.tokenType = created.tokenType;
			selected.personType = created.personType;
			selected.terrainType = created.terrainType;
			RebuildTokenCanvas();
			UpdateSelectedUi();
		};
		flyout.Items.Add( item );
	}

	async void AddTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( selected == null )
			return;
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( !tw.Accepted )
			return;
		TriggeredByCombo.ItemsSource = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();
		selected.triggeredByName = tw.EnteredTriggerName;
		UpdateSelectedUi();
	}

	async void EditExplorationButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( tile is not HexTile hex )
		{
			UpdateExploreStatus();
			return;
		}

		var originals = hex.CaptureStartingValues();
		var originalKeyName = hex.TranslationKeyName();
		var originalPrefix = hex.TranslationKeyPrefix();
		var sw = new StoryTextEditWindow( hex.flavorBookData.pages[0] );
		await sw.ShowDialog( this );
		if ( sw.Accepted )
			hex.flavorBookData.pages[0] = sw.StoryText;

		hex.UpdateKeysStartingWith( scenario.translationObserver, originalPrefix );
		hex.DecertifyChangedValues( scenario.translationObserver, originals, originalKeyName );
		UpdateExploreStatus();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e ) => Close();

	void TokenCanvas_OnPointerPressed( object? sender, PointerPressedEventArgs e )
	{
		var visual = e.Source as StyledElement;
		if ( visual?.DataContext is not Token tok || !_tokenShapes.TryGetValue( tok, out var shape ) )
			return;

		selected = tok;
		TokenList.SelectedItem = tok;
		HighlightSelection();

		_dragging = true;
		_dragToken = tok;
		var p = e.GetPosition( TokenCanvas );
		var gv = tok.position;
		_dragClickOffset = new Point( p.X - gv.X, p.Y - gv.Y );
		_lastPos = tok.position;
		e.Pointer.Capture( TokenCanvas );
		e.Handled = true;
	}

	void TokenCanvas_OnPointerMoved( object? sender, PointerEventArgs e )
	{
		if ( !_dragging || _dragToken == null || !_tokenShapes.TryGetValue( _dragToken, out var shape ) )
			return;

		var p = e.GetPosition( TokenCanvas );
		var clickPoint = new System.Windows.Vector(
			p.X - _dragClickOffset.X,
			p.Y - _dragClickOffset.Y );

		_dragToken.position = clickPoint;
		if ( _dragToken.position.X < 0 || _dragToken.position.Y < 0
		     || ( _dragToken.position.X + shape.Width ) > 512 || ( _dragToken.position.Y + shape.Height ) > 512 )
		{
			_dragToken.position = _lastPos;
		}

		_lastPos = _dragToken.position;
		ApplyTokenTransform( _dragToken );
		if ( selected == _dragToken )
			UpdateSelectedUi();
		e.Handled = true;
	}

	void TokenCanvas_OnPointerReleased( object? sender, PointerReleasedEventArgs e )
	{
		_dragging = false;
		_dragToken = null;
		e.Pointer.Capture( null );
	}

	void TokenCanvas_OnPointerWheelChanged( object? sender, PointerWheelEventArgs e )
	{
		if ( selected == null )
			return;
		if ( e.Delta.Y > 0 )
			RotateSelected( -1 );
		else if ( e.Delta.Y < 0 )
			RotateSelected( 1 );
		e.Handled = true;
	}

	void TokenEditorWindow_OnKeyDown( object? sender, KeyEventArgs e )
	{
		if ( selected == null )
			return;

		if ( e.Key == Key.PageUp )
			RotateSelected( -1 );
		else if ( e.Key == Key.PageDown )
			RotateSelected( 1 );
		else if ( e.Key == Key.Delete )
			RemoveTokenButton_Click( sender, new RoutedEventArgs() );
		else if ( e.Key == Key.Up )
			MoveSelected( 0, -1 );
		else if ( e.Key == Key.Down )
			MoveSelected( 0, 1 );
		else if ( e.Key == Key.Left )
			MoveSelected( -1, 0 );
		else if ( e.Key == Key.Right )
			MoveSelected( 1, 0 );
	}

	void MoveSelected( double dx, double dy )
	{
		if ( selected == null || !_tokenShapes.TryGetValue( selected, out var shape ) )
			return;

		var last = selected.position;
		selected.position = new System.Windows.Vector( selected.position.X + dx, selected.position.Y + dy );
		if ( selected.position.X < 0 || selected.position.Y < 0
		     || ( selected.position.X + shape.Width ) > 512 || ( selected.position.Y + shape.Height ) > 512 )
		{
			selected.position = last;
		}

		ApplyTokenTransform( selected );
		UpdateSelectedUi();
	}

	void RotateSelected( double direction )
	{
		if ( selected == null )
			return;
		if ( selected.tokenType != TokenType.Terrain )
			return;

		selected.angle += direction * selected.rotationAngle;
		selected.angle %= 360;
		ApplyTokenTransform( selected );
		UpdateSelectedUi();
	}

	void RebuildTokenCanvas()
	{
		_tokenShapes.Clear();
		TokenCanvas.Children.Clear();
		TokenCanvas.Children.Add( TileImage );

		foreach ( var token in tile.tokenList )
		{
			var shape = CreateTokenShape( token );
			shape.DataContext = token;
			_tokenShapes[token] = shape;
			TokenCanvas.Children.Add( shape );
			ApplyTokenTransform( token );
		}

		HighlightSelection();
	}

	void HighlightSelection()
	{
		foreach ( var kv in _tokenShapes )
		{
			var isSelected = kv.Key == selected;
			kv.Value.Stroke = isSelected ? Brushes.Red : Brushes.White;
			if ( isSelected )
			{
				TokenCanvas.Children.Remove( kv.Value );
				TokenCanvas.Children.Add( kv.Value );
			}
		}
	}

	void ApplyTokenTransform( Token token )
	{
		if ( !_tokenShapes.TryGetValue( token, out var shape ) )
			return;
		// WPF TokenEditorWindow path: translate + rotate with CenterX/Y = position (see JiMEditor/Models/Token.cs).
		// Avalonia: use canvas top-left for position and rotate around shape center so the pivot matches WPF visually.
		Canvas.SetLeft( shape, token.position.X );
		Canvas.SetTop( shape, token.position.Y );
		shape.RenderTransformOrigin = RelativePoint.Center;
		shape.RenderTransform = new RotateTransform( token.angle );
	}

	Shape CreateTokenShape( Token token )
	{
		Shape shape;
		if ( token.tokenType == TokenType.Terrain )
		{
			var terrain = token.terrainType;
			if ( new[] { TerrainType.Barrels, TerrainType.Barricade, TerrainType.Chest, TerrainType.Elevation, TerrainType.Log, TerrainType.Table }.Contains( terrain ) )
			{
				shape = new Rectangle { Width = 62, Height = 140 };
			}
			else if ( new[] { TerrainType.Fence, TerrainType.Stream, TerrainType.Trench, TerrainType.Wall }.Contains( terrain ) )
			{
				shape = new Rectangle { Width = 27, Height = 170 };
			}
			else if ( new[] { TerrainType.Boulder, TerrainType.Bush, TerrainType.FirePit, TerrainType.Rubble, TerrainType.Statue, TerrainType.Web }.Contains( terrain ) )
			{
				shape = new Ellipse { Width = 74, Height = 74 };
			}
			else if ( new[] { TerrainType.Fountain, TerrainType.Mist, TerrainType.Pit, TerrainType.Pond }.Contains( terrain ) )
			{
				shape = new Rectangle { Width = 150, Height = 150, RadiusX = 50, RadiusY = 50 };
			}
			else
			{
				shape = new Ellipse { Width = 50, Height = 50 };
			}
		}
		else
		{
			shape = new Ellipse { Width = 50, Height = 50 };
		}

		shape.HorizontalAlignment = HorizontalAlignment.Left;
		shape.VerticalAlignment = VerticalAlignment.Top;
		shape.Stroke = Brushes.White;
		shape.StrokeThickness = 4;
		shape.Fill = BuildTokenFill( token );
		return shape;
	}

	IBrush BuildTokenFill( Token token )
	{
		if ( token.tokenType == TokenType.Person )
		{
			var person = token.personType switch
			{
				PersonType.Human => "Human.png",
				PersonType.Elf => "Elf.png",
				PersonType.Hobbit => "Hobbit.png",
				PersonType.Dwarf => "Dwarf.png",
				_ => null
			};
			if ( person != null )
				return ImageBrushFromAssets( $"Tokens/{person}" );
		}

		if ( token.tokenType == TokenType.Terrain )
		{
			var terrain = token.terrainType switch
			{
				TerrainType.Barrels => "Barrels.png",
				TerrainType.Boulder => "Boulder.png",
				TerrainType.Bush => "Bush.png",
				TerrainType.FirePit => "FirePit.png",
				TerrainType.Mist => "Mist.png",
				TerrainType.Pit => "Pit.png",
				TerrainType.Statue => "Statue.png",
				TerrainType.Stream => "Stream.png",
				TerrainType.Table => "Table.png",
				TerrainType.Wall => "Wall.png",
				TerrainType.Elevation => "Elevation.png",
				TerrainType.Log => "Log.png",
				TerrainType.Rubble => "Rubble.png",
				TerrainType.Web => "Web.png",
				TerrainType.Barricade => "Barricade.png",
				TerrainType.Chest => "Chest.png",
				TerrainType.Fence => "Fence.png",
				TerrainType.Fountain => "Fountain.png",
				TerrainType.Pond => "Pond.png",
				TerrainType.Trench => "Trench.png",
				_ => null
			};
			if ( terrain != null )
				return ImageBrushFromAssets( $"Terrain/{terrain}" );
			return Brushes.Brown;
		}

		return token.tokenType switch
		{
			TokenType.Search => ImageBrushFromAssets( "Tokens/Search.png" ),
			TokenType.Person => ImageBrushFromAssets( "Tokens/Person.png" ),
			TokenType.Threat => ImageBrushFromAssets( "Tokens/Threat.png" ),
			TokenType.Darkness => ImageBrushFromAssets( "Tokens/Darkness.png" ),
			TokenType.DifficultGround => ImageBrushFromAssets( "Tokens/DifficultGround.png" ),
			TokenType.Fortified => ImageBrushFromAssets( "Tokens/Fortified.png" ),
			TokenType.Start => ImageBrushFromAssets( "Tokens/marker-start.png" ),
			_ => Brushes.Gray
		};
	}

	IBrush ImageBrushFromAssets( string relPath )
	{
		if ( _imageBrushCache.TryGetValue( relPath, out var b ) )
			return b;
		try
		{
			var uri = new Uri( $"avares://JiMEditor.Avalonia/Assets/{relPath}" );
			using var stream = AssetLoader.Open( uri );
			var bmp = new Bitmap( stream );
			var brush = new ImageBrush( bmp ) { Stretch = Stretch.Fill };
			_imageBrushCache[relPath] = brush;
			return brush;
		}
		catch
		{
			return Brushes.Gray;
		}
	}

	Bitmap? LoadTileBitmap()
	{
		try
		{
			var uri = new Uri( $"avares://JiMEditor.Avalonia/Assets/Tiles{tile.tileSide}/{tile.idNumber}.png" );
			using var stream = AssetLoader.Open( uri );
			return new Bitmap( stream );
		}
		catch
		{
			return null;
		}
	}

	protected override void OnClosed( EventArgs e )
	{
		if ( scenario != null )
			scenario.interactionObserver.CollectionChanged -= InteractionsCollectionChanged;
		base.OnClosed( e );
	}
}
