// Ported from: JiMEditor/MainWindow.xaml.cs (2026-04-04) — Avalonia shell; editor logic TODO-Avalonia-Reenable.
using System;
using System.ComponentModel;
using Avalonia.Controls;
using JiME;

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
		objectivesUC.SetItemsSource( scenario.objectiveObserver );
		activationsUC.SetItemsSource( scenario.activationsObserver );
		monsterModifiersUC.SetItemsSource( scenario.monsterModifierObserver );
		translationsUC.SetItemsSource( scenario.translationObserver );
	}

	void DetachSidebarLists()
	{
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

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		DetachSidebarLists();
		if ( scenario != null )
			scenario.PropertyChanged -= OnScenarioPropertyChanged;
		base.OnClosing( e );
	}
}
