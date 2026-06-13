// Ported from: JiMEditor/Views/ScenarioWindow.xaml.cs (2026-04-08) — partial Avalonia port.
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views;

public partial class ScenarioWindow : Window
{
	bool _closing;
	bool _currentType;
	bool _suppressObjectiveCombo;
	bool _suppressLastStandCombo;
	bool _suppressScenarioTypeRadio;
	bool _suppressCollectionCheck;

	public Scenario scenario { get; }
	public bool Accepted { get; private set; }

	public ScenarioWindow()
	{
		InitializeComponent();
		scenario = null!;
	}

	public ScenarioWindow( Scenario s ) : this()
	{
		scenario = s;
		_currentType = s.scenarioTypeJourney;
		DataContext = this;

		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		NameTextBox.Focus();
		NameTextBox.SelectAll();
		ResolutionComboBox.ItemsSource = scenario.resolutionObserver;
		LastStandFailedResolutionComboBox.ItemsSource = scenario.resolutionObserver;
		ResolutionComboBox.SelectedIndex = scenario.resolutionObserver.Count > 0 ? 0 : -1;

		_suppressObjectiveCombo = true;
		ObjectiveComboBox.SelectedItem = scenario.objectiveObserver.FirstOrDefault( o => o.dataName == scenario.objectiveName );
		_suppressObjectiveCombo = false;

		_suppressLastStandCombo = true;
		LastStandFailedResolutionComboBox.SelectedItem = string.IsNullOrEmpty( scenario.lastStandFailedResolution )
			? null
			: scenario.resolutionObserver.FirstOrDefault( r => r.dataName == scenario.lastStandFailedResolution );
		_suppressLastStandCombo = false;

		LoadCollections();
		LoadCampaignNotice();
	}

	void LoadCampaignNotice()
	{
		if ( scenario.campaignGUID == Guid.Empty )
			return;
		CampaignNotice.IsVisible = true;
		CampaignGuidText.Text = scenario.campaignGUID.ToString();
	}

	void LoadCollections()
	{
		if ( !scenario.collectionObserver.Contains( Collection.CORE_SET ) )
		{
			scenario.collectionObserver.Add( Collection.CORE_SET );
			scenario.RefilterGlobalTilePool();
		}

		_suppressCollectionCheck = true;
		try
		{
			CoreSetCheckBox.IsChecked = true;
			VillainsOfEriadorCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.VILLAINS_OF_ERIAJAR );
			ShadedPathsCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.SHADED_PATHS );
			DenizensInDarknessCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.DENIZENS_IN_DARKNESS );
			UnfurlingWarCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.UNFURLING_WAR );
			ScorchersOfTheWildsCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.SCORCHERS_OF_THE_WILDS );
			CustomCheckBox.IsChecked = scenario.collectionObserver.Contains( Collection.CUSTOM );
		}
		finally
		{
			_suppressCollectionCheck = false;
		}
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( _closing || !TryClose() )
			return;
		Accepted = true;
		_closing = true;
		Close();
	}

	bool TryClose()
	{
		if ( string.IsNullOrWhiteSpace( NameTextBox.Text ) )
		{
			MessageBox.Show( "The Scenario Title can't be empty.", "Invalid Scenario Title", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}

		scenario.scenarioName = NameTextBox.Text!.Trim();
		return true;
	}

	async void SpecialButton_Click( object? sender, RoutedEventArgs e )
	{
		var w = new StoryTextEditWindow( scenario.specialInstructions, windowTitle: "Edit Scenario Story" );
		await w.ShowDialog( this );
		if ( w.Accepted )
			scenario.specialInstructions = w.StoryText;
	}

	async void EditIntroButton_Click( object? sender, RoutedEventArgs e )
	{
		var initial = scenario.introBookData?.pages?.Count > 0 ? scenario.introBookData.pages[0] : "";
		var w = new StoryTextEditWindow( initial, windowTitle: "Edit Introduction Text" );
		await w.ShowDialog( this );
		if ( !w.Accepted )
			return;
		if ( scenario.introBookData == null )
			scenario.introBookData = new TextBookData( "Default Introduction Text" );
		if ( scenario.introBookData.pages.Count == 0 )
			scenario.introBookData.pages.Add( "" );
		scenario.introBookData.pages[0] = w.StoryText;
	}

	async void ImageButton_Click( object? sender, RoutedEventArgs e )
	{
		var w = new CoverImageEditorWindow( scenario.coverImage );
		await w.ShowDialog( this );
		if ( w.Accepted )
			scenario.coverImage = w.CoverImage;
	}

	async void AddResolutionButton_Click( object? sender, RoutedEventArgs e ) => await AddResolutionAsync();

	async Task AddResolutionAsync()
	{
		var data = new TextBookData();
		data.pages.Add( "Default Text" );
		data.triggerName = "None";
		var (accepted, endsSuccess) = await ResolutionEditorWindow.ShowEditAsync( this, scenario, data, isNew: true );
		if ( !accepted )
			return;
		if ( !scenario.AddResolution( data, endsSuccess ) )
		{
			MessageBox.Show( "A Resolution with this name already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		ResolutionComboBox.SelectedItem = data;
		scenario.PruneScenarioEnd();
	}

	async Task<(bool Accepted, bool EndsInSuccess)> RunResolutionEditorWithTranslationTailAsync( TextBookData data, bool isNew )
	{
		var originals = data.CaptureStartingValues();
		var originalPrefix = data.TranslationKeyPrefix();
		var originalKeyName = data.TranslationKeyName();
		var dlg = new ResolutionEditorWindow( scenario, data, isNew );
		await dlg.ShowDialog( this );
		data.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originals, originalPrefix, originalKeyName );
		return ( dlg.Accepted, dlg.EndsInSuccess );
	}

	async void ResolutionEditButton_Click( object? sender, RoutedEventArgs e ) => await EditResolutionAsync();

	async Task EditResolutionAsync()
	{
		if ( ResolutionComboBox.SelectedItem is not TextBookData text )
			return;
		if ( text.dataName == "None" )
		{
			MessageBox.Show( "The [None] Resolution cannot be modified.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		var (accepted, _) = await ResolutionEditorWindow.ShowEditAsync( this, scenario, text, isNew: false );
		if ( !accepted )
			return;
		scenario.PruneScenarioEnd();
	}

	void RemoveResolutionButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( ResolutionComboBox.SelectedItem is not TextBookData text )
			return;
		if ( text.dataName == "None" )
		{
			MessageBox.Show( "The [None] Resolution cannot be modified.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		var ret = MessageBox.Show(
			"Are you sure you want to delete this Resolution?\n\nALL ITS DATA WILL BE DELETED.",
			"Delete Resolution",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;

		var removedName = text.dataName;
		if ( scenario.resolutionObserver.Count > 1 )
			scenario.RemoveData( text );

		if ( scenario.lastStandFailedResolution == removedName )
		{
			scenario.lastStandFailedResolution = "";
			_suppressLastStandCombo = true;
			LastStandFailedResolutionComboBox.SelectedItem = null;
			_suppressLastStandCombo = false;
		}

		if ( scenario.resolutionObserver.Count >= 1 )
			ResolutionComboBox.SelectedIndex = 0;
		scenario.PruneScenarioEnd();
	}

	async void CollectionCheckBox_Changed( object? sender, RoutedEventArgs e )
	{
		if ( _suppressCollectionCheck )
			return;
		if ( sender is not CheckBox cb || cb.Content is not string name )
			return;
		var collection = Collection.FromName( name );
		var nowChecked = cb.IsChecked == true;

		if ( nowChecked )
		{
			if ( !scenario.collectionObserver.Contains( collection ) )
				scenario.collectionObserver.Add( collection );
			scenario.RefilterGlobalTilePool();
			return;
		}

		// Unchecking: confirm before removing from the scenario.
		if ( !scenario.collectionObserver.Contains( collection ) )
			return;

		var impact = ScenarioCollectionRemovalImpact.GetImpactForRemovingCollection( scenario, collection );
		var impactText = CollectionRemovalImpact.FormatForDialog( impact );
		var body = "ALL THIS COLLECTION'S TILES, TOKENS, ENEMIES, ITEMS, AND TITLES WILL BE REMOVED FROM THE SCENARIO IF YOU REMOVE THIS COLLECTION.\n\n";
		if ( string.IsNullOrWhiteSpace( impactText ) )
			body += "No matching references were found in Events (tiles elsewhere are not scanned yet).\n\n";
		else
			body += impactText + "\n\n";
		body += "Are you sure you want to remove this Collection from the scenario?";
		var ret = MessageBox.Show( body, "Remove Collection?", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
		{
			SetCollectionCheckBoxState( cb, true );
			return;
		}

		if ( !await MaybeSaveCopyBeforeDestructiveChangeAsync( async () =>
		     {
			     SetCollectionCheckBoxState( cb, true );
			     await Task.CompletedTask;
		     } ) )
			return;

		scenario.collectionObserver.Remove( collection );
		scenario.RefilterGlobalTilePool();
		SetCollectionCheckBoxState( cb, false );
	}

	void ObjectiveComboBox_SelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressObjectiveCombo )
			return;
		if ( ObjectiveComboBox.SelectedItem is Objective o )
			scenario.objectiveName = o.dataName;
	}

	void LastStandFailedResolutionComboBox_SelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressLastStandCombo )
			return;
		scenario.lastStandFailedResolution = LastStandFailedResolutionComboBox.SelectedItem is TextBookData r
			? r.dataName
			: "";
	}

	// WPF uses Click (once per click target). IsCheckedChanged on both radios fired twice per switch and re-ran this logic.
	async void RadioType_Checked( object? sender, RoutedEventArgs e )
	{
		if ( _suppressScenarioTypeRadio )
			return;
		// TwoWay binding has already applied the clicked choice to the scenario; compare to last committed type.
		if ( scenario.scenarioTypeJourney == _currentType )
			return;

		var requestedType = scenario.scenarioTypeJourney;
		var msg = requestedType
			? "The default Terrain: events will be removed since they are not needed for a Journey map."
			: "New default Terrain: events will be added since they can be useful for a Battle map.";
		var ret = MessageBox.Show(
			"Are you sure you want to change the Scenario Type?\n\nALL CHAPTER AND TILE DATA WILL BE RESET IF YOU SWITCH THE SCENARIO TYPE.\n\nYour custom Objectives, Events and Triggers will remain unchanged.\n\n" + msg,
			"Change Scenario Type", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret == MessageBoxResult.Yes )
		{
			if ( !await MaybeSaveCopyBeforeDestructiveChangeAsync( async () =>
			     {
				     RestoreScenarioTypeUiToCurrent();
				     await Task.CompletedTask;
			     } ) )
				return;

			_suppressScenarioTypeRadio = true;
			try
			{
				scenario.scenarioTypeJourney = requestedType;
			}
			finally
			{
				_suppressScenarioTypeRadio = false;
			}

			_currentType = scenario.scenarioTypeJourney;
			scenario.WipeChapters();
			if ( scenario.scenarioTypeJourney )
			{
				scenario.chapterObserver[0].ToJourneyTile();
				scenario.RemoveDefaultTerrainInteractions();
			}
			else
			{
				scenario.chapterObserver[0].ToSquareTile();
				scenario.AddDefaultTerrainInteractions();
			}

			scenario.RefilterGlobalTilePool();
		}
		else
		{
			RestoreScenarioTypeUiToCurrent();
		}
	}

	void SetCollectionCheckBoxState( CheckBox cb, bool isChecked )
	{
		_suppressCollectionCheck = true;
		try
		{
			cb.IsChecked = isChecked;
		}
		finally
		{
			_suppressCollectionCheck = false;
		}
	}

	void RestoreScenarioTypeUiToCurrent()
	{
		_suppressScenarioTypeRadio = true;
		try
		{
			scenario.scenarioTypeJourney = _currentType;
		}
		finally
		{
			_suppressScenarioTypeRadio = false;
		}
	}

	async Task<bool> MaybeSaveCopyBeforeDestructiveChangeAsync( Func<Task> restoreStateBeforeSaveAsync )
	{
		var ret = MessageBox.Show(
			"Do you want to save a copy first before continuing?",
			"Save Copy First?",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return true;

		await restoreStateBeforeSaveAsync();

		if ( Owner is not MainWindow mw )
		{
			MessageBox.Show(
				"Could not open Save Copy from this window.",
				"Save Copy",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		return await mw.SaveScenarioCopyForSafeguardAsync();
	}
}
