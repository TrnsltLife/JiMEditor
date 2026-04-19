// Ported from: JiMEditor/MainWindow.xaml.cs (2026-04-04) — Avalonia shell; editor logic TODO-Avalonia-Reenable.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views;

public partial class MainWindow : Window
{
	public Scenario scenario { get; private set; } = null!;

	/// <summary>Parameterless ctor required by Avalonia XAML; use <see cref="MainWindow(Scenario)"/> at runtime.</summary>
	public MainWindow()
	{
		InitializeComponent();
	}

	/// <summary>New standalone scenario (WPF: <c>new MainWindow(Guid.Empty)</c> from project window).</summary>
	public MainWindow( Guid campaignGuid ) : this( NewScenarioForEditor( campaignGuid ) )
	{
	}

	static Scenario NewScenarioForEditor( Guid campaignGuid )
	{
		var s = new Scenario();
		s.campaignGUID = campaignGuid;
		s.TriggerTitleChange( false );
		return s;
	}

	public MainWindow( Scenario s ) : this()
	{
		scenario = s;
		DataContext = scenario;
		appVersion.Text = Utils.appVersion;
		formatVersion.Text = Utils.formatVersion;
		WireSidebarLists();
		SyncTitleFromScenario();
		ScenarioNameEdit.Text = scenario.scenarioName ?? "";
		scenario.PropertyChanged += OnScenarioPropertyChanged;
	}

	void WireSidebarLists()
	{
		interactionsUC.SetItemsSource( scenario.interactionObserver );
		interactionsUC.onAddEvent += OnAddInteraction;
		interactionsUC.onRemoveEvent += OnRemoveInteraction;
		interactionsUC.onSettingsEvent += OnSettingsInteraction;
		interactionsUC.onDuplicateEvent += OnDuplicateInteraction;
		interactionsUC.onImportEvent += OnImportInteraction;
		interactionsUC.onExportEvent += OnExportInteraction;
		triggersUC.SetItemsSource( scenario.triggersObserver );
		triggersUC.onAddEvent += OnAddTrigger;
		triggersUC.onRemoveEvent += OnRemoveTrigger;
		triggersUC.onSettingsEvent += OnSettingsTrigger;
		triggersUC.onDuplicateEvent += OnDuplicateTrigger;
		objectivesUC.SetItemsSource( scenario.objectiveObserver );
		objectivesUC.onAddEvent += OnAddObjective;
		objectivesUC.onRemoveEvent += OnRemoveObjective;
		objectivesUC.onSettingsEvent += OnSettingsObjective;
		objectivesUC.onDuplicateEvent += OnDuplicateObjective;
		activationsUC.SetItemsSource( scenario.activationsObserver );
		activationsUC.onAddEvent += OnAddActivations;
		activationsUC.onRemoveEvent += OnRemoveActivations;
		activationsUC.onSettingsEvent += OnSettingsActivations;
		activationsUC.onDuplicateEvent += OnDuplicateActivations;
		activationsUC.onImportEvent += OnImportActivations;
		activationsUC.onExportEvent += OnExportActivations;
		monsterModifiersUC.SetItemsSource( scenario.monsterModifierObserver );
		monsterModifiersUC.onAddEvent += OnAddMonsterModifier;
		monsterModifiersUC.onRemoveEvent += OnRemoveMonsterModifier;
		monsterModifiersUC.onSettingsEvent += OnSettingsMonsterModifier;
		monsterModifiersUC.onDuplicateEvent += OnDuplicateMonsterModifier;
		monsterModifiersUC.onImportEvent += OnImportMonsterModifier;
		monsterModifiersUC.onExportEvent += OnExportMonsterModifier;
		translationsUC.SetItemsSource( scenario.translationObserver );
		translationsUC.onAddEvent += OnAddTranslation;
		translationsUC.onRemoveEvent += OnRemoveTranslation;
		translationsUC.onSettingsEvent += OnSettingsTranslation;
		translationsUC.onImportEvent += OnImportTranslation;
		translationsUC.onExportEvent += OnExportTranslation;
	}

	void DetachSidebarLists()
	{
		interactionsUC.onAddEvent -= OnAddInteraction;
		interactionsUC.onRemoveEvent -= OnRemoveInteraction;
		interactionsUC.onSettingsEvent -= OnSettingsInteraction;
		interactionsUC.onDuplicateEvent -= OnDuplicateInteraction;
		interactionsUC.onImportEvent -= OnImportInteraction;
		interactionsUC.onExportEvent -= OnExportInteraction;
		monsterModifiersUC.onAddEvent -= OnAddMonsterModifier;
		monsterModifiersUC.onRemoveEvent -= OnRemoveMonsterModifier;
		monsterModifiersUC.onSettingsEvent -= OnSettingsMonsterModifier;
		monsterModifiersUC.onDuplicateEvent -= OnDuplicateMonsterModifier;
		monsterModifiersUC.onImportEvent -= OnImportMonsterModifier;
		monsterModifiersUC.onExportEvent -= OnExportMonsterModifier;
		translationsUC.onAddEvent -= OnAddTranslation;
		translationsUC.onRemoveEvent -= OnRemoveTranslation;
		translationsUC.onSettingsEvent -= OnSettingsTranslation;
		translationsUC.onImportEvent -= OnImportTranslation;
		translationsUC.onExportEvent -= OnExportTranslation;
		objectivesUC.onAddEvent -= OnAddObjective;
		objectivesUC.onRemoveEvent -= OnRemoveObjective;
		objectivesUC.onSettingsEvent -= OnSettingsObjective;
		objectivesUC.onDuplicateEvent -= OnDuplicateObjective;
		triggersUC.onAddEvent -= OnAddTrigger;
		triggersUC.onRemoveEvent -= OnRemoveTrigger;
		triggersUC.onSettingsEvent -= OnSettingsTrigger;
		triggersUC.onDuplicateEvent -= OnDuplicateTrigger;
		activationsUC.onAddEvent -= OnAddActivations;
		activationsUC.onRemoveEvent -= OnRemoveActivations;
		activationsUC.onSettingsEvent -= OnSettingsActivations;
		activationsUC.onDuplicateEvent -= OnDuplicateActivations;
		activationsUC.onImportEvent -= OnImportActivations;
		activationsUC.onExportEvent -= OnExportActivations;
		objectivesUC.DetachItemsSource();
		interactionsUC.DetachItemsSource();
		triggersUC.DetachItemsSource();
		activationsUC.DetachItemsSource();
		monsterModifiersUC.DetachItemsSource();
		translationsUC.DetachItemsSource();
	}

	void OnScenarioPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName is nameof( Scenario.titleChangedToken )
		     or nameof( Scenario.scenarioName )
		     or nameof( Scenario.isDirty )
		     or nameof( Scenario.fileName )
		     or nameof( Scenario.projectType ) )
			SyncTitleFromScenario();
		if ( e.PropertyName == nameof( Scenario.scenarioName ) && !ScenarioNameEdit.IsVisible )
			ScenarioNameEdit.Text = scenario.scenarioName ?? "";
	}

	void ScenarioNameText_PointerPressed( object? sender, PointerPressedEventArgs e )
	{
		if ( !e.GetCurrentPoint( ScenarioNameText ).Properties.IsLeftButtonPressed )
			return;
		ScenarioNameText.IsVisible = false;
		ScenarioNameEdit.IsVisible = true;
		ScenarioNameEdit.Text = scenario.scenarioName ?? "";
		ScenarioNameEdit.Focus();
		ScenarioNameEdit.SelectAll();
	}

	void ScenarioNameEdit_KeyDown( object? sender, KeyEventArgs e )
	{
		if ( e.Key == Key.Enter )
		{
			CommitScenarioNameEdit();
			e.Handled = true;
		}
		else if ( e.Key == Key.Escape )
		{
			CancelScenarioNameEdit();
			e.Handled = true;
		}
	}

	void ScenarioNameEdit_LostFocus( object? sender, RoutedEventArgs e ) => CommitScenarioNameEdit();

	void CommitScenarioNameEdit()
	{
		var newName = ( ScenarioNameEdit.Text ?? "" ).Trim();
		if ( newName.Length == 0 )
		{
			MessageBox.Show( "The Scenario name cannot be an empty string.", "Invalid Scenario Name", MessageBoxButton.OK, MessageBoxImage.Information );
			ScenarioNameEdit.Text = scenario.scenarioName ?? "";
			ScenarioNameEdit.Focus();
			ScenarioNameEdit.SelectAll();
			return;
		}

		scenario.scenarioName = newName;
		ScenarioNameText.IsVisible = true;
		ScenarioNameEdit.IsVisible = false;
	}

	void CancelScenarioNameEdit()
	{
		ScenarioNameEdit.Text = scenario.scenarioName ?? "";
		ScenarioNameText.IsVisible = true;
		ScenarioNameEdit.IsVisible = false;
	}

	/// <summary>Mirrors WPF <see cref="TitleConverter"/> (JiME/Converters.cs).</summary>
	void SyncTitleFromScenario()
	{
		if ( scenario == null )
			return;
		var t = scenario.titleChangedToken;
		if ( t == null )
		{
			Title = UserJourneyPaths.EditorWindowTitlePrefix;
			return;
		}

		string name = string.IsNullOrEmpty( t.Item2 ) ? "Untitled" : t.Item2;
		Title = $"{UserJourneyPaths.EditorWindowTitlePrefix} - {t.Item4} Scenario - {name}{( t.Item1 ? "*" : "" )}";
	}

	async Task AddTriggerAsync()
	{
		var w = new TriggerEditorWindow( scenario );
		await w.ShowDialog( this );
	}

	async void AddTriggerToolbarButton_Click( object? sender, RoutedEventArgs e ) => await AddTriggerAsync();

	async void NewTileBlockToolbarButton_Click( object? sender, RoutedEventArgs e )
	{
		var cw = new ChapterPropertiesWindow( scenario, null );
		var originalValues = cw.chapter.CaptureStartingValues();
		var originalPrefix = cw.chapter.TranslationKeyPrefix();
		var originalKeyName = cw.chapter.TranslationKeyName();
		await cw.ShowDialog( this );
		cw.chapter.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		if ( cw.Accepted )
			scenario.AddChapter( cw.chapter );
	}

	async void TileEditButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button b || b.DataContext is not Chapter c )
			return;
		await OpenChapterTileEditorAsync( c );
	}

	async void ChapterPropsButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button b || b.DataContext is not Chapter c )
			return;
		await OpenChapterPropertiesEditorAsync( c );
	}

	void RemoveChapterButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button b || b.DataContext is not Chapter c )
			return;
		var ret = MessageBox.Show(
			"Are you sure you want to delete this Tile Block?\n\nALL TILES AND TOKENS WILL BE DELETED.",
			"Delete Tile Block",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;

		foreach ( var tile in c.tileObserver )
			scenario.globalTilePool.Add( tile.idNumber );
		var sorter = new TileSorter();
		var foo = scenario.globalTilePool.ToList();
		foo.Sort( sorter );
		scenario.globalTilePool.Clear();
		foreach ( var n in foo )
			scenario.globalTilePool.Add( n );
		scenario.RemoveData( c );
	}

	async Task OpenChapterPropertiesEditorAsync( Chapter c )
	{
		var originalValues = c.CaptureStartingValues();
		var originalPrefix = c.TranslationKeyPrefix();
		var originalKeyName = c.TranslationKeyName();
		var cw = new ChapterPropertiesWindow( scenario, c );
		await cw.ShowDialog( this );
		c.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
	}

	async Task OpenChapterTileEditorAsync( Chapter c )
	{
		if ( scenario.scenarioTypeJourney )
		{
			// Random tile pool editor. Chapters default isRandomTiles=false in the model until Properties is used;
			// the fixed/manual hex layout editor is not ported yet (TODO-Avalonia-Reenable).
			var tp = new TilePoolEditorWindow( scenario, c );
			await tp.ShowDialog( this );
		}
		else
		{
			// TODO-Avalonia-Reenable: TileEditorWindow for battle maps.
		}
	}

	async void OnAddTrigger( object? sender, EventArgs e ) => await AddTriggerAsync();

	async void AddObjectiveToolbarButton_Click( object? sender, RoutedEventArgs e ) => await AddObjectiveAsync();

	async Task AddObjectiveAsync()
	{
		var ow = new ObjectiveEditorWindow( scenario, new Objective( "Default Short Name - Change This" ) );
		await ow.ShowDialog( this );
		if ( ow.Accepted )
			scenario.AddObjective( ow.objective );
	}

	async void OnAddObjective( object? sender, EventArgs e ) => await AddObjectiveAsync();

	async Task AddActivationsAsync()
	{
		int maxId = scenario.activationsObserver.Max( a => a.id );
		int newId = Math.Max( maxId + 1, MonsterActivations.START_OF_CUSTOM_ACTIVATIONS );
		var aew = new ActivationsEditorWindow( scenario, new MonsterActivations( newId ), false );
		await aew.ShowDialog( this );
		if ( aew.Accepted )
			scenario.AddActivations( aew.activations );
	}

	async void OnAddActivations( object? sender, EventArgs e ) => await AddActivationsAsync();

	void OnRemoveActivations( object? sender, EventArgs e )
	{
		if ( activationsUC.DataListView.SelectedItem is not MonsterActivations act )
			return;
		if ( act.id >= MonsterActivations.START_OF_CUSTOM_ACTIVATIONS )
		{
			var ret = MessageBox.Show(
				"Are you sure you want to delete this Enemy Attack Group?\n\nALL DESCRIPTIONS AND DAMAGE WILL BE DELETED.",
				"Delete Enemy Attack Group", MessageBoxButton.YesNo, MessageBoxImage.Question );
			if ( ret == MessageBoxResult.Yes )
			{
				var idx = activationsUC.DataListView.SelectedIndex;
				scenario.RemoveData( act );
				activationsUC.DataListView.SelectedIndex =
					scenario.activationsObserver.Count > 0 ? Math.Max( idx - 1, 0 ) : -1;
			}
		}
		else
		{
			MessageBox.Show(
				"You can't delete the default Enemy Attack Groups, but you can copy them and modify the copy.",
				"Cannot Delete Enemy Attack Group", MessageBoxButton.OK, MessageBoxImage.Information );
		}
	}

	async void OnSettingsActivations( object? sender, EventArgs e )
	{
		if ( activationsUC.DataListView.SelectedItem is not MonsterActivations act )
			return;
		if ( act.id >= MonsterActivations.START_OF_CUSTOM_ACTIVATIONS )
		{
			var ow = new ActivationsEditorWindow( scenario, act, false, readOnly: false );
			await ow.ShowDialog( this );
		}
		else
		{
			MessageBox.Show(
				"You can't modify the default Enemy Attack Groups, but you can copy them and modify the copy.",
				"Cannot Modify Enemy Attack Group", MessageBoxButton.OK, MessageBoxImage.Information );
			var view = new ActivationsEditorWindow( scenario, act, false, readOnly: true );
			await view.ShowDialog( this );
		}
	}

	async void OnDuplicateActivations( object? sender, EventArgs e )
	{
		if ( activationsUC.DataListView.SelectedItem is not MonsterActivations src )
			return;
		int maxId = scenario.activationsObserver.Max( a => a.id );
		int newId = Math.Max( maxId + 1, MonsterActivations.START_OF_CUSTOM_ACTIVATIONS );
		var act = src.Clone( newId );
		var aew = new ActivationsEditorWindow( scenario, act, true );
		await aew.ShowDialog( this );
		if ( aew.Accepted )
			scenario.activationsObserver.Add( act );
	}

	void OnImportActivations( object? sender, EventArgs e )
	{
		var activationItem = new ImportManager().ImportMonsterActivations( scenario );
		if ( activationItem != null )
			scenario.activationsObserver.Add( activationItem );
	}

	void OnExportActivations( object? sender, EventArgs e )
	{
		if ( activationsUC.DataListView.SelectedItem is not MonsterActivations selected )
			return;
		new ExportManager().ExportMonsterActivation( scenario, selected );
	}

	void OnRemoveObjective( object? sender, EventArgs e )
	{
		if ( scenario.objectiveObserver.Count > 1 )
		{
			var ret = MessageBox.Show(
				"Are you sure you want to delete this Objective?\n\nALL ITS INFORMATION WILL BE DELETED.",
				"Delete Objective", MessageBoxButton.YesNo, MessageBoxImage.Question );
			if ( ret == MessageBoxResult.Yes )
			{
				var idx = objectivesUC.DataListView.SelectedIndex;
				if ( idx >= 0 && objectivesUC.DataListView.SelectedItem is Objective o )
					scenario.RemoveData( o );
				objectivesUC.DataListView.SelectedIndex = scenario.objectiveObserver.Count > 0 ? 0 : -1;
			}
		}
		else
		{
			MessageBox.Show( "There must be at least one Objective.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}

	async Task AddMonsterModifierAsync()
	{
		int maxId = scenario.monsterModifierObserver.Max( a => a.id );
		int newId = Math.Max( maxId + 1, MonsterModifier.START_OF_CUSTOM_MODIFIERS );
		var mmew = new MonsterModifierEditorWindow( scenario, new MonsterModifier( newId ), false );
		await mmew.ShowDialog( this );
		if ( mmew.Accepted )
			scenario.AddMonsterModifier( mmew.modifier );
	}

	async void OnAddMonsterModifier( object? sender, EventArgs e ) => await AddMonsterModifierAsync();

	void OnRemoveMonsterModifier( object? sender, EventArgs e )
	{
		if ( monsterModifiersUC.DataListView.SelectedItem is not MonsterModifier mod )
			return;
		if ( mod.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS )
		{
			var ret = MessageBox.Show(
				"Are you sure you want to delete this Enemy Bonus?",
				"Delete Enemy Bonus", MessageBoxButton.YesNo, MessageBoxImage.Question );
			if ( ret == MessageBoxResult.Yes )
			{
				var idx = monsterModifiersUC.DataListView.SelectedIndex;
				scenario.RemoveData( mod );
				monsterModifiersUC.DataListView.SelectedIndex =
					scenario.monsterModifierObserver.Count > 0 ? Math.Max( idx - 1, 0 ) : -1;
			}
		}
		else
		{
			MessageBox.Show(
				"You can't delete the default Enemy Bonuses, but you can copy them and modify the copy.",
				"Cannot Delete Enemy Bonus", MessageBoxButton.OK, MessageBoxImage.Information );
		}
	}

	async void OnSettingsMonsterModifier( object? sender, EventArgs e )
	{
		if ( monsterModifiersUC.DataListView.SelectedItem is not MonsterModifier mod )
			return;
		if ( mod.id >= MonsterModifier.START_OF_CUSTOM_MODIFIERS )
			await OpenMonsterModifierEditorAsync( mod, readOnly: false );
		else
		{
			MessageBox.Show(
				"You can't modify the default Enemy Bonuses, but you can copy them and modify the copy.",
				"Cannot Modify Enemy Bonus", MessageBoxButton.OK, MessageBoxImage.Information );
			await OpenMonsterModifierEditorAsync( mod, readOnly: true );
		}
	}

	async Task OpenMonsterModifierEditorAsync( MonsterModifier mod, bool readOnly )
	{
		if ( readOnly )
		{
			var view = new MonsterModifierEditorWindow( scenario, mod, isNew: false, readOnly: true );
			await view.ShowDialog( this );
			return;
		}

		var originalValues = mod.CaptureStartingValues();
		var originalPrefix = mod.TranslationKeyPrefix();
		var originalKeyName = mod.TranslationKeyName();
		var editor = new MonsterModifierEditorWindow( scenario, mod, false );
		await editor.ShowDialog( this );
		mod.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		if ( editor.Accepted )
			monsterModifiersUC.RefreshSortedItems();
	}

	async void OnDuplicateMonsterModifier( object? sender, EventArgs e )
	{
		if ( monsterModifiersUC.DataListView.SelectedItem is not MonsterModifier src )
			return;
		int maxId = scenario.monsterModifierObserver.Max( a => a.id );
		int newId = Math.Max( maxId + 1, MonsterModifier.START_OF_CUSTOM_MODIFIERS );
		var mod = src.Clone( newId );
		var mmew = new MonsterModifierEditorWindow( scenario, mod, true );
		await mmew.ShowDialog( this );
		if ( mmew.Accepted )
			scenario.AddMonsterModifier( mod );
	}

	void OnImportMonsterModifier( object? sender, EventArgs e )
	{
		var modifierItem = new ImportManager().ImportMonsterModifier( scenario );
		if ( modifierItem != null )
			scenario.AddMonsterModifier( modifierItem );
	}

	void OnExportMonsterModifier( object? sender, EventArgs e )
	{
		if ( monsterModifiersUC.DataListView.SelectedItem is not MonsterModifier selected )
			return;
		new ExportManager().ExportMonsterModifier( scenario, selected );
	}

	async void OnDuplicateObjective( object? sender, EventArgs e )
	{
		if ( objectivesUC.DataListView.SelectedItem is not Objective src )
			return;
		var obj = src.Clone();
		var oew = new ObjectiveEditorWindow( scenario, obj, true );
		await oew.ShowDialog( this );
		if ( oew.Accepted )
			scenario.objectiveObserver.Add( obj );
	}

	async void OnSettingsObjective( object? sender, EventArgs e )
	{
		if ( objectivesUC.DataListView.SelectedItem is not Objective o )
			return;
		await OpenObjectiveEditorAsync( o );
	}

	async Task OpenObjectiveEditorAsync( Objective o )
	{
		Dictionary<string, string> originalValues = o.CaptureStartingValues();
		string originalPrefix = o.TranslationKeyPrefix();
		string originalKeyName = o.TranslationKeyName();
		var ow = new ObjectiveEditorWindow( scenario, o, false );
		await ow.ShowDialog( this );
		o.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
	}

	void OnRemoveTrigger( object? sender, EventArgs e )
	{
		if ( triggersUC.DataListView.SelectedItem is not Trigger strigger )
			return;
		var selected = strigger.dataName;
		var used = scenario.IsTriggerUsed( selected );
		if ( used != null )
		{
			MessageBox.Show(
				$"The selected Trigger [{selected}] is being used by [{used.Item2}] called [{used.Item1}].",
				"Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		if ( strigger.isCampaignTrigger )
		{
			MessageBox.Show( "Campaign Triggers cannot be removed.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		var ret = MessageBox.Show( "Are you sure you want to delete this Trigger?", "Delete Trigger", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;

		if ( triggersUC.DataListView.SelectedIndex != -1 )
			scenario.RemoveData( strigger );
		triggersUC.DataListView.SelectedIndex = scenario.triggersObserver.Count > 0 ? 0 : -1;
		scenario.TriggerTitleChange( true );
	}

	async void OnSettingsTrigger( object? sender, EventArgs e )
	{
		if ( triggersUC.DataListView.SelectedItem is not Trigger t )
			return;
		var w = new TriggerEditorWindow( scenario, t.dataName );
		await w.ShowDialog( this );
	}

	async void OnDuplicateTrigger( object? sender, EventArgs e )
	{
		if ( triggersUC.DataListView.SelectedItem is not Trigger src )
			return;
		var trig = src.Clone();
		var w = new TriggerEditorWindow( scenario, trig, true );
		await w.ShowDialog( this );
	}

	async void OnAddTranslation( object? sender, EventArgs e ) => await AddTranslationAsync();

	async Task AddTranslationAsync()
	{
		var defaultTranslation = scenario.CollectTranslationItemsAsDictionary();
		var initial = new Translation();
		var w = new TranslationEditorWindow( scenario, defaultTranslation, initial, true );
		await w.ShowDialog( this );
		if ( w.Accepted )
			scenario.AddTranslation( initial );
	}

	void OnRemoveTranslation( object? sender, EventArgs e )
	{
		if ( translationsUC.DataListView.SelectedItem is not Translation translation )
			return;
		var title = $"Delete Translation: {translation.langName} / {translation.dataName}";
		var ret = MessageBox.Show(
			$"Are you sure you want to delete this Translation?\n\n{translation.langName} / {translation.dataName}\n\nALL TRANSLATION TEXT FOR THIS LANGUAGE WILL BE DELETED.\n\nThis cannot be undone.",
			title, MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;
		var ret2 = MessageBox.Show(
			$"Please Reconfirm:\n\nAre you REALLY, REALLY sure you want to delete this Translation?\n\n{translation.langName} / {translation.dataName}\n\nALL TRANSLATION TEXT FOR THIS LANGUAGE WILL BE DELETED.\n\nThis cannot be undone.",
			title, MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret2 != MessageBoxResult.Yes )
			return;
		var idx = translationsUC.DataListView.SelectedIndex;
		scenario.RemoveData( translation );
		translationsUC.DataListView.SelectedIndex = scenario.translationObserver.Count > 0 ? Math.Max( idx - 1, 0 ) : -1;
	}

	async void OnSettingsTranslation( object? sender, EventArgs e )
	{
		if ( translationsUC.DataListView.SelectedItem is not Translation t )
			return;
		var defaultTranslation = scenario.CollectTranslationItemsAsDictionary();
		var w = new TranslationEditorWindow( scenario, defaultTranslation, t, false );
		await w.ShowDialog( this );
	}

	void OnImportTranslation( object? sender, EventArgs e )
	{
		new ImportManager().ImportTranslation( scenario.translationObserver );
	}

	void OnExportTranslation( object? sender, EventArgs e )
	{
		if ( translationsUC.DataListView.SelectedItem is not Translation selectedTranslation )
			return;
		var translationItemsForExport = scenario.CollectTranslationItemsForExport( selectedTranslation );
		var translationForExport = new TranslationForExport( selectedTranslation.dataName, selectedTranslation.langName, translationItemsForExport );
		new ExportManager().ExportTranslation( scenario.scenarioName, translationForExport );
	}

	async void ScenarioSettingsButton_Click( object? sender, RoutedEventArgs e )
	{
		var sw = new ScenarioWindow( scenario );
		await sw.ShowDialog( this );
	}

	async void ThreatsToolbarButton_Click( object? sender, RoutedEventArgs e )
	{
		var w = new ThreatsWindow( scenario );
		await w.ShowDialog( this );
	}

	void OnAddInteraction( object? sender, EventArgs e )
	{
		var flyout = new MenuFlyout();
		var entries = new (string Header, Func<Task> Add)[]
		{
			( "Conditional Event", AddConditionalInteractionAsync ),
			( "Corruption Event", AddCorruptionInteractionAsync ),
			( "Decision Event", AddDecisionInteractionAsync ),
			( "Dialog Event", AddDialogInteractionAsync ),
			( "Enemy Event", AddThreatInteractionAsync ),
			( "Item Event", AddItemInteractionAsync ),
			( "Multi-Event", AddMultiEventInteractionAsync ),
			( "Persistent Event", AddPersistentInteractionAsync ),
			( "Replace Token Event", AddReplaceTokenInteractionAsync ),
			( "Reward Event", AddRewardInteractionAsync ),
			( "Stat Test Event", AddStatTestInteractionAsync ),
			( "Story Branch", AddBranchInteractionAsync ),
			( "Text Event", AddTextInteractionAsync ),
			( "Title Event", AddTitleInteractionAsync )
		};
		foreach ( var (header, add) in entries.OrderBy( x => x.Header, StringComparer.OrdinalIgnoreCase ) )
		{
			var item = new MenuItem { Header = header };
			item.Click += async (_, _) => await add();
			flyout.Items.Add( item );
		}

		var placement = sender as Control ?? interactionsUC.AddInteractionButton;
		flyout.ShowAt( placement );
	}

	void AddEventToolbarButton_Click( object? sender, RoutedEventArgs e ) =>
		OnAddInteraction( sender, e );

	async Task AddDialogInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateDialogInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddTextInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateTextInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddBranchInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateBranchInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddRewardInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateRewardInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddPersistentInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreatePersistentInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddConditionalInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateConditionalInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddItemInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateItemInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddTitleInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateTitleInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddCorruptionInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateCorruptionInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddDecisionInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateDecisionInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddMultiEventInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateMultiEventInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddThreatInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateThreatInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddStatTestInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateStatTestInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async Task AddReplaceTokenInteractionAsync()
	{
		var (accepted, created) = await EventEditorFactory.TryCreateReplaceTokenInteractionAsync( this, scenario );
		if ( accepted )
			scenario.AddInteraction( created );
	}

	async void OnSettingsInteraction( object? sender, EventArgs e )
	{
		if ( interactionsUC.DataListView.SelectedItem is not IInteraction i )
			return;
		await EventEditorFactory.TryEditInteractionAsync( this, scenario, i );
		interactionsUC.RefreshSortedItems();
	}

	async void OnDuplicateInteraction( object? sender, EventArgs e )
	{
		if ( interactionsUC.DataListView.SelectedItem is not IInteraction src )
		{
			MessageBox.Show(
				"Duplicating this event type is not implemented yet in Avalonia.",
				"Not implemented",
				MessageBoxButton.OK,
				MessageBoxImage.Information );
			return;
		}

		switch ( src )
		{
			case DialogInteraction d:
			{
				var clone = d.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new DialogInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case RewardInteraction r:
			{
				var clone = r.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new RewardInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case PersistentTokenInteraction p:
			{
				var clone = p.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new PersistentInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case ConditionalInteraction c:
			{
				var clone = c.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new ConditionalInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case ItemInteraction i:
			{
				var clone = i.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new ItemInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case TitleInteraction t:
			{
				var clone = t.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new TitleInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case CorruptionInteraction c:
			{
				var clone = c.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new CorruptionInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case DecisionInteraction d:
			{
				var clone = d.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new DecisionInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case MultiEventInteraction m:
			{
				var clone = m.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new MultiEventInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case ThreatInteraction t:
			{
				var clone = t.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new ThreatInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case TextInteraction tx:
			{
				var clone = tx.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new TextInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case BranchInteraction br:
			{
				var clone = br.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new BranchInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case ReplaceTokenInteraction rt:
			{
				var clone = rt.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new ReplaceTokenInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			case TestInteraction te:
			{
				var clone = te.Clone();
				var originalValues = clone.CaptureStartingValues();
				var originalPrefix = clone.TranslationKeyPrefix();
				var originalKeyName = clone.TranslationKeyName();
				var w = new TestInteractionWindow( scenario, clone, showCancelButton: true );
				await w.ShowDialog( this );
				clone.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( clone );
				break;
			}
			default:
				MessageBox.Show(
					"Duplicating this event type is not implemented yet in Avalonia.",
					"Not implemented",
					MessageBoxButton.OK,
					MessageBoxImage.Information );
				break;
		}
	}

	void OnRemoveInteraction( object? sender, EventArgs e )
	{
		if ( interactionsUC.DataListView.SelectedItem is StartInteraction )
		{
			MessageBox.Show(
				"You can't delete the Starting Position event.",
				"Cannot Delete Start Position Event",
				MessageBoxButton.OK,
				MessageBoxImage.Information );
			return;
		}

		var ret = MessageBox.Show(
			"Are you sure you want to delete this Event?\n\nALL OF ITS DATA WILL BE DELETED.",
			"Delete Event",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;

		var idx = interactionsUC.DataListView.SelectedIndex;
		if ( idx >= 0 && interactionsUC.DataListView.SelectedItem is IInteraction item )
			scenario.RemoveData( item );
		interactionsUC.DataListView.SelectedIndex = scenario.interactionObserver.Count > 0 ? 0 : -1;
	}

	async void OnImportInteraction( object? sender, EventArgs e )
	{
		var interactionItem = new ImportManager().ImportEvent( scenario );
		if ( interactionItem == null )
			return;
		switch ( interactionItem )
		{
			case DialogInteraction d:
			{
				var originalValues = d.CaptureStartingValues();
				var originalPrefix = d.TranslationKeyPrefix();
				var originalKeyName = d.TranslationKeyName();
				var w = new DialogInteractionWindow( scenario, d, showCancelButton: true );
				await w.ShowDialog( this );
				d.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( d );
				break;
			}
			case TextInteraction tx:
			{
				var originalValues = tx.CaptureStartingValues();
				var originalPrefix = tx.TranslationKeyPrefix();
				var originalKeyName = tx.TranslationKeyName();
				var w = new TextInteractionWindow( scenario, tx, showCancelButton: true );
				await w.ShowDialog( this );
				tx.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( tx );
				break;
			}
			case BranchInteraction br:
			{
				var originalValues = br.CaptureStartingValues();
				var originalPrefix = br.TranslationKeyPrefix();
				var originalKeyName = br.TranslationKeyName();
				var w = new BranchInteractionWindow( scenario, br, showCancelButton: true );
				await w.ShowDialog( this );
				br.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( br );
				break;
			}
			case ReplaceTokenInteraction rt:
			{
				var originalValues = rt.CaptureStartingValues();
				var originalPrefix = rt.TranslationKeyPrefix();
				var originalKeyName = rt.TranslationKeyName();
				var w = new ReplaceTokenInteractionWindow( scenario, rt, showCancelButton: true );
				await w.ShowDialog( this );
				rt.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( rt );
				break;
			}
			case TestInteraction te:
			{
				var originalValues = te.CaptureStartingValues();
				var originalPrefix = te.TranslationKeyPrefix();
				var originalKeyName = te.TranslationKeyName();
				var w = new TestInteractionWindow( scenario, te, showCancelButton: true );
				await w.ShowDialog( this );
				te.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( te );
				break;
			}
			case RewardInteraction r:
			{
				var originalValues = r.CaptureStartingValues();
				var originalPrefix = r.TranslationKeyPrefix();
				var originalKeyName = r.TranslationKeyName();
				var w = new RewardInteractionWindow( scenario, r, showCancelButton: true );
				await w.ShowDialog( this );
				r.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( r );
				break;
			}
			case PersistentTokenInteraction p:
			{
				var originalValues = p.CaptureStartingValues();
				var originalPrefix = p.TranslationKeyPrefix();
				var originalKeyName = p.TranslationKeyName();
				var w = new PersistentInteractionWindow( scenario, p, showCancelButton: true );
				await w.ShowDialog( this );
				p.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( p );
				break;
			}
			case ConditionalInteraction c:
			{
				var originalValues = c.CaptureStartingValues();
				var originalPrefix = c.TranslationKeyPrefix();
				var originalKeyName = c.TranslationKeyName();
				var w = new ConditionalInteractionWindow( scenario, c, showCancelButton: true );
				await w.ShowDialog( this );
				c.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( c );
				break;
			}
			case ItemInteraction i:
			{
				var originalValues = i.CaptureStartingValues();
				var originalPrefix = i.TranslationKeyPrefix();
				var originalKeyName = i.TranslationKeyName();
				var w = new ItemInteractionWindow( scenario, i, showCancelButton: true );
				await w.ShowDialog( this );
				i.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( i );
				break;
			}
			case TitleInteraction t:
			{
				var originalValues = t.CaptureStartingValues();
				var originalPrefix = t.TranslationKeyPrefix();
				var originalKeyName = t.TranslationKeyName();
				var w = new TitleInteractionWindow( scenario, t, showCancelButton: true );
				await w.ShowDialog( this );
				t.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( t );
				break;
			}
			case CorruptionInteraction c:
			{
				var originalValues = c.CaptureStartingValues();
				var originalPrefix = c.TranslationKeyPrefix();
				var originalKeyName = c.TranslationKeyName();
				var w = new CorruptionInteractionWindow( scenario, c, showCancelButton: true );
				await w.ShowDialog( this );
				c.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( c );
				break;
			}
			case DecisionInteraction d:
			{
				var originalValues = d.CaptureStartingValues();
				var originalPrefix = d.TranslationKeyPrefix();
				var originalKeyName = d.TranslationKeyName();
				var w = new DecisionInteractionWindow( scenario, d, showCancelButton: true );
				await w.ShowDialog( this );
				d.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( d );
				break;
			}
			case MultiEventInteraction m:
			{
				var originalValues = m.CaptureStartingValues();
				var originalPrefix = m.TranslationKeyPrefix();
				var originalKeyName = m.TranslationKeyName();
				var w = new MultiEventInteractionWindow( scenario, m, showCancelButton: true );
				await w.ShowDialog( this );
				m.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( m );
				break;
			}
			case ThreatInteraction t:
			{
				var originalValues = t.CaptureStartingValues();
				var originalPrefix = t.TranslationKeyPrefix();
				var originalKeyName = t.TranslationKeyName();
				var w = new ThreatInteractionWindow( scenario, t, showCancelButton: true );
				await w.ShowDialog( this );
				t.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				if ( w.Accepted )
					scenario.AddInteraction( t );
				break;
			}
			default:
				MessageBox.Show(
					"Importing this event type is not implemented yet in Avalonia.",
					"Not implemented",
					MessageBoxButton.OK,
					MessageBoxImage.Information );
				break;
		}
	}

	void OnExportInteraction( object? sender, EventArgs e )
	{
		if ( interactionsUC.DataListView.SelectedItem is not InteractionBase selected )
			return;
		new ExportManager().ExportEvent( scenario, selected );
	}

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		DetachSidebarLists();
		if ( scenario != null )
			scenario.PropertyChanged -= OnScenarioPropertyChanged;
		base.OnClosing( e );
	}
}
