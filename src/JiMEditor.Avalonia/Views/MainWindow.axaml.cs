// Ported from: JiMEditor/MainWindow.xaml.cs (2026-04-04) — Avalonia shell; editor logic TODO-Avalonia-Reenable.
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

	public MainWindow( Scenario s ) : this()
	{
		scenario = s;
		DataContext = scenario;
		SyncTitleFromScenario();
		scenario.PropertyChanged += OnScenarioPropertyChanged;
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
			Title = "Your Journey";
			return;
		}

		string name = string.IsNullOrEmpty( t.Item2 ) ? "Untitled" : t.Item2;
		Title = $"Your Journey - {t.Item4} Scenario - {name}{( t.Item1 ? "*" : "" )}";
	}

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		if ( scenario != null )
			scenario.PropertyChanged -= OnScenarioPropertyChanged;
		base.OnClosing( e );
	}
}
