// Ported from: JiMEditor/MainWindow.xaml.cs (2026-04-04) — Avalonia shell; editor logic TODO-Avalonia-Reenable.
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		monsterModifiersUC.SetItemsSource( scenario.monsterModifierObserver );
		translationsUC.SetItemsSource( scenario.translationObserver );
	}

	void DetachSidebarLists()
	{
		objectivesUC.onAddEvent -= OnAddObjective;
		objectivesUC.onRemoveEvent -= OnRemoveObjective;
		objectivesUC.onSettingsEvent -= OnSettingsObjective;
		objectivesUC.onDuplicateEvent -= OnDuplicateObjective;
		triggersUC.onAddEvent -= OnAddTrigger;
		triggersUC.onRemoveEvent -= OnRemoveTrigger;
		triggersUC.onSettingsEvent -= OnSettingsTrigger;
		triggersUC.onDuplicateEvent -= OnDuplicateTrigger;
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

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		DetachSidebarLists();
		if ( scenario != null )
			scenario.PropertyChanged -= OnScenarioPropertyChanged;
		base.OnClosing( e );
	}
}
