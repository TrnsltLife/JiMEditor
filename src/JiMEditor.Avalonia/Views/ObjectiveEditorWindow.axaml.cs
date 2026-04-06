// Ported from: JiMEditor/Views/ObjectiveEditorWindow.xaml.cs (2026-04-06) — Avalonia UI.
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views;

public partial class ObjectiveEditorWindow : Window, INotifyPropertyChanged
{
	string _shortName = "";
	bool _suppressComboSync;

	public Scenario scenario { get; private set; } = null!;
	public Objective objective { get; private set; } = null!;

	public string shortName
	{
		get => _shortName;
		set
		{
			if ( _shortName == value )
				return;
			_shortName = value;
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( shortName ) ) );
		}
	}

	public bool Accepted { get; private set; }

	public ObjectiveEditorWindow()
	{
		InitializeComponent();
	}

	public ObjectiveEditorWindow( Scenario s, Objective obj, bool isNew = true )
		: this()
	{
		DataContext = this;
		scenario = s;
		objective = obj;
		shortName = obj.dataName;

		TriggeredByCombo.ItemsSource = s.triggersObserver;
		CompletionTriggerCombo.ItemsSource = s.triggersObserver;
		NextTriggerCombo.ItemsSource = s.triggersObserver;

		TriggeredByCombo.SelectionChanged += TriggeredByCombo_OnSelectionChanged;
		CompletionTriggerCombo.SelectionChanged += CompletionTriggerCombo_OnSelectionChanged;
		NextTriggerCombo.SelectionChanged += NextTriggerCombo_OnSelectionChanged;

		CancelButton.IsVisible = isNew;

		// Hydrate reminder/reward text boxes after the visual tree exists (Opened alone can run before nested controls are ready).
		Loaded += OnObjectiveEditorWindowLoaded;
	}

	void OnObjectiveEditorWindowLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnObjectiveEditorWindowLoaded;
		if ( objective == null || scenario == null )
			return;

		PopulateEditableFieldsFromObjective();
		SyncAllTriggerCombosFromObjective();
		ShortNameTextBox?.Focus();
		ShortNameTextBox?.SelectAll();
	}

	/// <summary>Reminder and numeric rewards are not bound; push model → UI when the dialog opens.</summary>
	void PopulateEditableFieldsFromObjective()
	{
		ObjectiveReminderTextBox.Text = objective.objectiveReminder ?? "";
		LoreRewardTextBox.Text = objective.loreReward.ToString( CultureInfo.InvariantCulture );
		XpRewardTextBox.Text = objective.xpReward.ToString( CultureInfo.InvariantCulture );
		ThreatRewardTextBox.Text = objective.threatReward.ToString( CultureInfo.InvariantCulture );
	}

	void SyncAllTriggerCombosFromObjective()
	{
		var previousSuppress = _suppressComboSync;
		_suppressComboSync = true;
		try
		{
			SelectTriggerComboItem( TriggeredByCombo, objective.triggeredByName );
			SelectTriggerComboItem( CompletionTriggerCombo, objective.triggerName );
			SelectTriggerComboItem( NextTriggerCombo, objective.nextTrigger );
		}
		finally
		{
			_suppressComboSync = previousSuppress;
		}
	}

	static void SelectTriggerComboItem( ComboBox cb, string? dataName )
	{
		if ( string.IsNullOrEmpty( dataName ) )
		{
			cb.SelectedItem = null;
			return;
		}

		cb.SelectedItem = cb.ItemsSource is System.Collections.IEnumerable src
			? src.Cast<object>().OfType<Trigger>().FirstOrDefault( t => t.dataName == dataName )
			: null;
	}

	void TriggeredByCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressComboSync )
			return;
		if ( TriggeredByCombo.SelectedItem is Trigger t )
			objective.triggeredByName = t.dataName;
	}

	void CompletionTriggerCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressComboSync )
			return;
		if ( CompletionTriggerCombo.SelectedItem is Trigger t )
			objective.triggerName = t.dataName;
	}

	void NextTriggerCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressComboSync )
			return;
		if ( NextTriggerCombo.SelectedItem is Trigger t )
			objective.nextTrigger = t.dataName;
		else
			objective.nextTrigger = "None";
	}

	/// <summary>Caller must set <see cref="_suppressComboSync"/> when needed (e.g. after adding a trigger) so clearing ItemsSource does not run <see cref="NextTriggerCombo_OnSelectionChanged"/> with null and reset <see cref="Objective.nextTrigger"/>.</summary>
	void RebindTriggerComboItemsSources()
	{
		var src = scenario.triggersObserver;
		TriggeredByCombo.ItemsSource = null;
		CompletionTriggerCombo.ItemsSource = null;
		NextTriggerCombo.ItemsSource = null;
		TriggeredByCombo.ItemsSource = src;
		CompletionTriggerCombo.ItemsSource = src;
		NextTriggerCombo.ItemsSource = src;
	}

	async Task AddTriggerAndSelectAsync( ComboBox targetCombo )
	{
		var w = new TriggerEditorWindow( scenario );
		await w.ShowDialog( this );
		if ( !w.Accepted || scenario.triggersObserver.Count == 0 )
			return;
		var last = scenario.triggersObserver[^1];

		if ( ReferenceEquals( targetCombo, TriggeredByCombo ) )
			objective.triggeredByName = last.dataName;
		else if ( ReferenceEquals( targetCombo, CompletionTriggerCombo ) )
			objective.triggerName = last.dataName;
		else if ( ReferenceEquals( targetCombo, NextTriggerCombo ) )
			objective.nextTrigger = last.dataName;

		_suppressComboSync = true;
		try
		{
			RebindTriggerComboItemsSources();
			SyncAllTriggerCombosFromObjective();
		}
		finally
		{
			_suppressComboSync = false;
		}
	}

	async void AddTriggeredBy_Click( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( TriggeredByCombo );

	async void AddCompletionTrigger_Click( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( CompletionTriggerCombo );

	async void AddNextTrigger_Click( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( NextTriggerCombo );

	async void EditSummary_Click( object? sender, RoutedEventArgs e )
	{
		var initial = objective.textBookData.pages.Count > 0 ? objective.textBookData.pages[0] : "";
		var dlg = new ObjectiveSummaryEditWindow( initial );
		await dlg.ShowDialog( this );
		if ( !dlg.Accepted )
			return;
		while ( objective.textBookData.pages.Count < 1 )
			objective.textBookData.pages.Add( "" );
		objective.textBookData.pages[0] = dlg.SummaryText;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		objective.objectiveReminder = ( ObjectiveReminderTextBox.Text ?? "" ).Trim();
		objective.loreReward = ParseRewardInt( LoreRewardTextBox.Text );
		objective.xpReward = ParseRewardInt( XpRewardTextBox.Text );
		objective.threatReward = ParseRewardInt( ThreatRewardTextBox.Text );

		objective.dataName = ( shortName ?? "" ).Trim();

		if ( string.IsNullOrEmpty( objective.dataName ) || string.IsNullOrEmpty( objective.objectiveReminder ) )
		{
			MessageBox.Show( "The Objective Name and Objective Reminder cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		if ( scenario.IsDuplicate( objective ) )
		{
			MessageBox.Show( $"An Objective with name [{objective.dataName}] already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		Accepted = true;
		scenario.TriggerTitleChange( true );
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	static int ParseRewardInt( string? text )
	{
		if ( string.IsNullOrWhiteSpace( text ) )
			return 0;
		return int.TryParse( text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n ) ? n : 0;
	}
}
