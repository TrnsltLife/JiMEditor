// Ported from: JiMEditor/Views/ScenarioWindow.xaml.cs - threat panel (2026-04-08)
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views;

public class ThreatItem
{
	public IInteraction trigger { get; set; }
	public Threat threat { get; set; }
	public ThreatList parent;

	public ThreatItem( ThreatList p, IInteraction trigger, Threat threat )
	{
		parent = p;
		this.trigger = trigger;
		this.threat = threat;
	}
}

public class ThreatList : INotifyPropertyChanged
{
	public ObservableCollection<ThreatItem> threatItemCollection { get; set; } = new();

	Threat _theThreat;
	public Threat theThreat
	{
		get => _theThreat;
		set
		{
			_theThreat = value;
			PropChanged( nameof( theThreat ) );
		}
	}

	ThreatItem? _selectedItem;
	public ThreatItem? selectedItem
	{
		get => _selectedItem;
		set
		{
			_selectedItem = value;
			PropChanged( nameof( selectedItem ) );
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	void PropChanged( string name ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

	public void ReloadTriggers( IInteraction[] collection )
	{
		threatItemCollection.Clear();
		foreach ( var inter in collection )
			threatItemCollection.Add( new ThreatItem( this, inter, theThreat ) );

		selectedItem = threatItemCollection.FirstOrDefault( x => x.trigger.dataName == theThreat.triggerName )
		               ?? threatItemCollection.FirstOrDefault( x => x.trigger.dataName == "None" );
	}

	public ThreatList( Threat threat, IInteraction[] collection )
	{
		_theThreat = threat;
		ReloadTriggers( collection );
	}
}

public partial class ThreatsWindow : Window
{
	public ObservableCollection<ThreatList> threatCollection { get; } = new();
	public Scenario scenario { get; }

	public ThreatsWindow()
	{
		InitializeComponent();
		scenario = null!;
	}

	public ThreatsWindow( Scenario s ) : this()
	{
		scenario = s;
		DataContext = this;
		Loaded += OnLoaded;
		Closing += OnWindowClosing;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		LoadThreats();
	}

	void OnWindowClosing( object? sender, WindowClosingEventArgs e ) => UpdateThreatPanel();

	void LoadThreats()
	{
		var sorted = scenario.threatObserver.OrderBy( key => key.threshold ).ToList();
		for ( var i = 0; i < sorted.Count; i++ )
			scenario.threatObserver[i] = sorted[i];

		threatCollection.Clear();
		foreach ( var t in scenario.threatObserver )
			threatCollection.Add( new ThreatList( t, scenario.interactionObserver.Where( x => !x.isTokenInteraction ).ToArray() ) );
	}

	void UpdateThreatPanel()
	{
		for ( var i = 0; i < threatCollection.Count; i++ )
			scenario.threatObserver[i] = threatCollection[i].theThreat;
		var sorted = scenario.threatObserver.OrderBy( key => key.threshold ).ToList();
		for ( var i = 0; i < sorted.Count; i++ )
			scenario.threatObserver[i] = sorted[i];
		LoadThreats();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		UpdateThreatPanel();
		Close();
	}

	void AddThreatButton_Click( object? sender, RoutedEventArgs e )
	{
		var i = 1;
		var name = "Threat";
		while ( scenario.threatObserver.Any( x => x.dataName == name ) )
		{
			i++;
			name = $"Threat {i}";
		}

		scenario.threatObserver.Add( new Threat( name ) );
		LoadThreats();
	}

	void RemoveThreatButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button { DataContext: ThreatList tl } )
			return;
		var ret = MessageBox.Show( "Are you sure you want to delete this Threat?\n\nALL ITS DATA WILL BE DELETED.", "Delete Threat Block", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;
		scenario.RemoveData( tl.theThreat );
		LoadThreats();
	}

	void EventInteractionComboBox_SelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( sender is not ComboBox cb || cb.SelectedItem is not ThreatItem item )
			return;
		item.threat.triggerName = item.trigger.dataName;
		item.parent.theThreat = item.threat;
		if ( scenario.threatObserver.Any( x => x.triggerName == item.trigger.dataName ) )
		{
			item.trigger.triggerName = "None";
			item.trigger.isTokenInteraction = false;
		}
	}
}
