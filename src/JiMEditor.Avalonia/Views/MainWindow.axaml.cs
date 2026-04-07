// Ported from: JiMEditor/MainWindow.xaml.cs (2026-04-04) — Avalonia shell; editor logic TODO-Avalonia-Reenable.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
		WireSidebarLists();
		SyncTitleFromScenario();
		scenario.PropertyChanged += OnScenarioPropertyChanged;
	}

	void WireSidebarLists()
	{
		interactionsUC.SetItemsSource( scenario.interactionObserver );
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
			var ow = new ActivationsEditorWindow( scenario, act, false );
			await ow.ShowDialog( this );
		}
		else
		{
			MessageBox.Show(
				"You can't modify the default Enemy Attack Groups, but you can copy them and modify the copy.",
				"Cannot Modify Enemy Attack Group", MessageBoxButton.OK, MessageBoxImage.Information );
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

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		DetachSidebarLists();
		if ( scenario != null )
			scenario.PropertyChanged -= OnScenarioPropertyChanged;
		base.OnClosing( e );
	}
}
