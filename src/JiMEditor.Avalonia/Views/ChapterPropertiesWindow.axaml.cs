// Ported from: JiMEditor/Views/ChapterPropertiesWindow.xaml.cs (2026-04-17) — Avalonia UI.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views;

public partial class ChapterPropertiesWindow : Window
{
	readonly BaseTile _emptyTile = new( 0 );
	string _oldName = "";
	bool _closing;
	bool _suppressNumText;
	int _numInters;
	int _requestedInters;

	public Chapter chapter { get; set; } = null!;
	public Scenario scenario { get; set; } = null!;

	public ObservableCollection<string> randomInteractions { get; } = new();

	public ObservableCollection<Chapter> chapterAttachOptions { get; } = new();

	public ObservableCollection<BaseTile> chapterTileAttachOptions { get; } = new();

	public IEnumerable<DensityPreference> DensityPreferenceValues =>
		Enum.GetValues( typeof( DensityPreference ) ).Cast<DensityPreference>();

	public bool Accepted { get; private set; }

	public ChapterPropertiesWindow()
	{
		InitializeComponent();
	}

	public ChapterPropertiesWindow( Scenario s, Chapter? c = null )
		: this()
	{
		scenario = s;
		CancelButton.IsVisible = c == null;

		chapter = c ?? new Chapter( "New Block" );
		_oldName = c?.dataName ?? "";
		// Set DataContext after chapter/scenario assignment so initial bindings (e.g. chapter.dataName) hydrate correctly.
		DataContext = this;

		if ( chapter.dataName != "Start" )
			PreExplorePanel.IsVisible = false;
		else
		{
			PreExplorePanel.IsVisible = true;
			HintBlock.IsVisible = false;
			FlavorBox.IsVisible = false;
			ExploreFirstBox.IsVisible = false;
		}

		BuildRandomInteractions();
		RandInterCombo.ItemsSource = randomInteractions;
		RandInterCombo.SelectedItem = chapter.randomInteractionGroup;

		PopulateSortedTriggerCombos();
		SyncTriggerSelectionsFromModel();

		chapterAttachOptions.Clear();
		foreach ( var ch in scenario.chapterObserver.Where( it => it.dataName != chapter.dataName ) )
			chapterAttachOptions.Add( ch );
		chapterAttachOptions.Add( new Chapter( "Random" ) );

		AttachHintCombo.ItemsSource = chapterAttachOptions;
		SelectAttachHintCombo();

		UpdateChapterTileAttachOptions( chapter.attachHint, chapter.attachTileHint );

		_requestedInters = chapter.randomInteractionGroupCount;
		_suppressNumText = true;
		NumIntersUsedTextBox.Text = _requestedInters.ToString();
		_suppressNumText = false;

		RecalcGroupInteractionCount();

		Loaded += OnLoaded;
		Closing += OnClosing;
		NoFlavorCheckBox.IsCheckedChanged += NoFlavorCheckBox_OnIsCheckedChanged;
		AttachToTileCombo.SelectionChanged += AttachToTileCombo_OnSelectionChanged;
	}

	void AttachToTileCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( AttachToTileCombo.SelectedItem is BaseTile bt )
			chapter.attachTileHint = bt.idNumber;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		NameTextBox.Focus();
		NameTextBox.SelectAll();
		UpdateTexts();
		RefreshFlavorPreview();
	}

	void OnClosing( object? sender, WindowClosingEventArgs e )
	{
		if ( _closing )
			return;
		ApplyFromUiToChapter();
		if ( !ValidateChapter() )
			e.Cancel = true;
	}

	void NoFlavorCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		RefreshFlavorPreview();
	}

	void RefreshFlavorPreview()
	{
		if ( chapter.noFlavorText )
			FlavorPreview.PseudoHtml = "";
		else
			FlavorPreview.PseudoHtml = chapter.flavorBookData.pages.Count > 0 ? chapter.flavorBookData.pages[0] : "";
	}

	void PopulateSortedTriggerCombos()
	{
		var sorted = scenario.triggersObserver
			.OrderByDescending( t => t.isEmpty )
			.ThenBy( t => t.dataName )
			.ToList();
		TriggeredByCombo.ItemsSource = sorted;
		ExploreTriggerCombo.ItemsSource = sorted;
		ExploredAllTilesTriggerCombo.ItemsSource = sorted;
	}

	void SyncTriggerSelectionsFromModel()
	{
		SelectTriggerCombo( TriggeredByCombo, chapter.triggeredBy );
		SelectTriggerCombo( ExploreTriggerCombo, chapter.exploreTrigger );
		SelectTriggerCombo( ExploredAllTilesTriggerCombo, chapter.exploredAllTilesTrigger );
	}

	static void SelectTriggerCombo( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not IEnumerable<object> src )
			return;
		cb.SelectedItem = string.IsNullOrEmpty( dataName )
			? null
			: src.OfType<Trigger>().FirstOrDefault( t => t.dataName == dataName );
	}

	void SelectAttachHintCombo()
	{
		var match = chapterAttachOptions.FirstOrDefault( c => c.dataName == chapter.attachHint );
		AttachHintCombo.SelectedItem = match;
	}

	void BuildRandomInteractions()
	{
		var ri = from inter in scenario.interactionObserver where inter.isTokenInteraction select inter.dataName;
		var hash = new HashSet<string>( new[] { "None" } );
		foreach ( var item in ri )
		{
			var groupKey = InteractionGroupNaming.TryGetGroupKey( item );
			if ( groupKey == null )
				continue;
			var isToken = scenario.interactionObserver
				.Where( x => x.dataName.EndsWith( groupKey, StringComparison.Ordinal ) )
				.All( x => x.isTokenInteraction );
			if ( isToken )
				hash.Add( groupKey );
		}

		randomInteractions.Clear();
		foreach ( var s in hash.OrderBy( x => x ) )
			randomInteractions.Add( s );
	}

	public void UpdateChapterTileAttachOptions( string attachName, int attachTile )
	{
		var c = scenario.chapterObserver.FirstOrDefault( x => x.dataName == attachName );

		chapterTileAttachOptions.Clear();
		if ( c != null )
		{
			chapterTileAttachOptions.Add( _emptyTile );
			foreach ( BaseTile bt in c.tileObserver )
				chapterTileAttachOptions.Add( bt );
		}

		AttachToTileCombo.ItemsSource = chapterTileAttachOptions;
		AttachToTileCombo.SelectedItem = chapterTileAttachOptions.FirstOrDefault( x => x.idNumber == attachTile );
	}

	void AttachHintCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( AttachHintCombo.SelectedItem is Chapter attachTileBlock )
		{
			chapter.attachHint = attachTileBlock.dataName;
			UpdateChapterTileAttachOptions( attachTileBlock.dataName, chapter.attachTileHint );
		}
	}

	void ApplyFromUiToChapter()
	{
		chapter.dataName = NameTextBox.Text?.Trim() ?? "";
		chapter.triggeredBy = TriggeredByCombo.SelectedItem is Trigger tb ? tb.dataName : "None";
		chapter.exploreTrigger = ExploreTriggerCombo.SelectedItem is Trigger te ? te.dataName : "None";
		chapter.exploredAllTilesTrigger = ExploredAllTilesTriggerCombo.SelectedItem is Trigger ta ? ta.dataName : "None";
		chapter.randomInteractionGroup = RandInterCombo.SelectedItem as string ?? "None";
		if ( AttachToTileCombo.SelectedItem is BaseTile bt )
			chapter.attachTileHint = bt.idNumber;

		Int32.TryParse( NumIntersUsedTextBox.Text, out var n );
		chapter.randomInteractionGroupCount = n;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		ApplyFromUiToChapter();
		if ( !ValidateChapter() )
			return;
		_closing = true;
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		foreach ( var tile in chapter.tileObserver )
			scenario.globalTilePool.Add( tile.idNumber );
		var sorted = scenario.globalTilePool.OrderBy( k => k ).ToList();
		scenario.globalTilePool.Clear();
		foreach ( var x in sorted )
			scenario.globalTilePool.Add( x );

		_closing = true;
		Close();
	}

	bool ValidateChapter()
	{
		if ( chapter.dataName == "Start" && !chapter.isEmpty )
		{
			MessageBox.Show( "Only the first Tile Block may be named 'Start'.", "Invalid Chapter Name", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( chapter.dataName == "Random" )
		{
			MessageBox.Show( "No Tile Block may be named 'Random'.", "Invalid Chapter Name", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( chapter.dataName == "None" )
		{
			MessageBox.Show( "No Tile Block may be named 'None'.", "Invalid Chapter Name", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( chapter.dataName != _oldName )
		{
			if ( scenario.chapterObserver.Count( c => c.GUID != chapter.GUID && c.dataName == chapter.dataName ) > 0 )
			{
				MessageBox.Show( "A Tile Block with this name already exists. Give this Tile Block a unique name.", "Invalid Chapter Name", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}
		}

		if ( !chapter.usesRandomGroups )
		{
			chapter.randomInteractionGroup = "None";
			chapter.randomInteractionGroupCount = 0;
		}

		if ( chapter.dataName != "Start"
		     && chapter.triggeredBy == "None" )
		{
			MessageBox.Show( "'Triggered By' cannot be set to None.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( chapter.tileObserver.Count == 0 )
		{
			MessageBox.Show( "There are no tiles assigned to this tile block. At least one tile is required.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		if ( string.IsNullOrEmpty( chapter.dataName.Trim() ) )
		{
			MessageBox.Show( "'Chapter Name' cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		return true;
	}

	async void EditFlavorButton_Click( object? sender, RoutedEventArgs e )
	{
		var page0 = chapter.flavorBookData.pages.Count > 0 ? chapter.flavorBookData.pages[0] : "";
		var sw = new StoryTextEditWindow( page0 );
		await sw.ShowDialog( this );
		if ( sw.Accepted )
		{
			if ( chapter.flavorBookData.pages.Count == 0 )
				chapter.flavorBookData.pages.Add( sw.StoryText );
			else
				chapter.flavorBookData.pages[0] = sw.StoryText;
			RefreshFlavorPreview();
		}
	}

	async void TileEditButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( !scenario.scenarioTypeJourney )
		{
			var tw = new TileEditorWindow( scenario, chapter );
			await tw.ShowDialog( this );
			UpdateTexts();
			return;
		}

		if ( chapter.isRandomTiles )
		{
			var tp = new TilePoolEditorWindow( scenario, chapter );
			await tp.ShowDialog( this );
			UpdateTexts();
		}
		else
		{
			var tw = new TileEditorWindow( scenario, chapter );
			await tw.ShowDialog( this );
			UpdateTexts();
		}
	}

	async void AddTriggerByButton_Click( object? sender, RoutedEventArgs e )
	{
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( tw.Accepted )
		{
			PopulateSortedTriggerCombos();
			chapter.triggeredBy = tw.EnteredTriggerName;
			SyncTriggerSelectionsFromModel();
		}
	}

	async void AddExploreTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( tw.Accepted )
		{
			PopulateSortedTriggerCombos();
			chapter.exploreTrigger = tw.EnteredTriggerName;
			SyncTriggerSelectionsFromModel();
		}
	}

	async void AddExploredAllTilesTriggerButton_Click( object? sender, RoutedEventArgs e )
	{
		var tw = new TriggerEditorWindow( scenario );
		await tw.ShowDialog( this );
		if ( tw.Accepted )
		{
			PopulateSortedTriggerCombos();
			chapter.exploredAllTilesTrigger = tw.EnteredTriggerName;
			SyncTriggerSelectionsFromModel();
		}
	}

	async void GroupHelp_Click( object? sender, RoutedEventArgs e ) =>
		await HelpWindow.ShowDialogAsync( this, HelpType.Grouping, 1 );

	void UseRandomCheckBox_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( UseRandomCheckBox.IsChecked != true )
		{
			chapter.randomInteractionGroup = "None";
			RandInterCombo.SelectedItem = "None";
			chapter.randomInteractionGroupCount = 0;
			_suppressNumText = true;
			NumIntersUsedTextBox.Text = "0";
			_suppressNumText = false;
		}
	}

	void RandomToggleCheckBox_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( chapter.isRandomTiles )
			return;

		MessageBox.Show(
			"Switching back to fixed tiles will automatically change any random tiles using a Random Side to Side A.\n\nReminder: Be sure to use the Tile Editor to properly place your tiles, now that they will be in fixed, user-defined positions.",
			"Switching to Fixed Tiles",
			MessageBoxButton.OK,
			MessageBoxImage.Information );

		foreach ( var tile in chapter.tileObserver.OfType<HexTile>() )
		{
			if ( tile.tileSide == "Random" )
				tile.tileSide = "A";
		}
	}

	void RecalcGroupInteractionCount()
	{
		if ( RandInterCombo.SelectedItem is not string sel )
		{
			_numInters = 0;
			return;
		}

		_numInters = scenario.interactionObserver.Count( x => x.dataName != "None" && x.dataName.EndsWith( sel ) );
	}

	void RandInterCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		RecalcGroupInteractionCount();
		UpdateTexts();
	}

	void NumIntersUsedTextBox_OnTextChanged( object? sender, TextChangedEventArgs e )
	{
		if ( _suppressNumText )
			return;
		Int32.TryParse( NumIntersUsedTextBox.Text, out _requestedInters );
		UpdateTexts();
	}

	void UpdateTexts()
	{
		var fixedTokenCount = 0;
		if ( chapter.tileObserver.Count > 0 )
			fixedTokenCount = chapter.tileObserver.Select( x => (BaseTile)x ).Select( x => x.tokenList.Count ).Aggregate( ( acc, cur ) => acc + cur );

		SelectedInfoText.Text = $"There are {_numInters} Events in the selected Group.";
		FixedCountText.Text = $"There are {fixedTokenCount} fixed Tokens in this Block.";

		var numspaces = chapter.tileObserver.Aggregate( 0, ( acc, cur ) =>
			acc + ( ( cur.idNumber == 998 || cur.idNumber == 999 ) ? 33 : ( cur.idNumber / 100 ) % 10 ) );
		SpaceInfoText.Text = $"There are {numspaces} total spaces available on this Block's tiles to place Tokens.";

		var max = Math.Min( _numInters, numspaces - fixedTokenCount );
		NumIntersUsedLabel.Text = $"Randomly use how many of the Events from the selected Interaction Group, up to a maximum of {max}:";

		max = Math.Min( _requestedInters, max );
		_suppressNumText = true;
		NumIntersUsedTextBox.Text = max.ToString();
		_suppressNumText = false;
	}
}
