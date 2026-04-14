// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml — triggers (2026-04-13).
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventTriggerSection : UserControl
{
	bool _suppressSelectionSync;
	Scenario? _scenario;
	InteractionBase? _interaction;

	public EventTriggerSection()
	{
		InitializeComponent();
		TriggeredByCombo.SelectionChanged += TriggeredByCombo_OnSelectionChanged;
		TriggerAfterCombo.SelectionChanged += TriggerAfterCombo_OnSelectionChanged;
	}

	public void Attach( Scenario scenario, InteractionBase interaction )
	{
		_scenario = scenario;
		_interaction = interaction;
		var sorted = SortTriggers( scenario.triggersObserver );
		TriggeredByCombo.ItemsSource = sorted;
		TriggerAfterCombo.ItemsSource = sorted;
	}

	static IEnumerable<Trigger> SortTriggers( System.Collections.IEnumerable raw ) =>
		EventDialogTriggerSort.Sort( raw );

	public void Hydrate()
	{
		if ( _interaction == null )
			return;
		SyncTriggeredBy();
		SyncTriggerAfter();
	}

	public void SetTriggeredByEnabled( bool enabled )
	{
		TriggeredByCombo.IsEnabled = enabled;
		AddTriggeredByButton.IsEnabled = enabled;
	}

	void SyncTriggeredBy() => SelectCombo( TriggeredByCombo, _interaction?.triggerName );

	void SyncTriggerAfter() => SelectCombo( TriggerAfterCombo, _interaction?.triggerAfterName );

	static void SelectCombo( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not System.Collections.IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}

		var list = src.Cast<Trigger>().ToList();
		if ( string.IsNullOrEmpty( dataName ) || dataName == "None" )
		{
			cb.SelectedItem = list.FirstOrDefault( t => t.dataName == "None" );
			return;
		}

		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName );
	}

	public void SyncAllFromModel()
	{
		var previousSuppress = _suppressSelectionSync;
		_suppressSelectionSync = true;
		try
		{
			SyncTriggeredBy();
			SyncTriggerAfter();
		}
		finally
		{
			_suppressSelectionSync = previousSuppress;
		}
	}

	void TriggeredByCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressSelectionSync || _interaction == null )
			return;
		if ( TriggeredByCombo.SelectedItem is Trigger t )
			_interaction.triggerName = t.dataName;
		else
			_interaction.triggerName = "None";
	}

	void TriggerAfterCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppressSelectionSync || _interaction == null )
			return;
		if ( TriggerAfterCombo.SelectedItem is Trigger t )
			_interaction.triggerAfterName = t.dataName;
		else
			_interaction.triggerAfterName = "None";
	}

	async void AddTriggeredBy_OnClick( object? sender, RoutedEventArgs e ) =>
		await TryAddTriggerAndSelectAsync( TriggeredByCombo, name => { if ( _interaction != null ) _interaction.triggerName = name; } );

	async void AddTriggerAfter_OnClick( object? sender, RoutedEventArgs e ) =>
		await TryAddTriggerAndSelectAsync( TriggerAfterCombo, name => { if ( _interaction != null ) _interaction.triggerAfterName = name; } );

	async Task TryAddTriggerAndSelectAsync( ComboBox targetCombo, System.Action<string> assign )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted )
			return;
		var name = tw.EnteredTriggerName;
		if ( string.IsNullOrEmpty( name ) )
			return;
		Attach( _scenario, _interaction! );
		assign( name );
		_suppressSelectionSync = true;
		try
		{
			targetCombo.ItemsSource = SortTriggers( _scenario.triggersObserver );
			SelectCombo( targetCombo, name );
		}
		finally
		{
			_suppressSelectionSync = false;
		}
	}
}
