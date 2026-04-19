// Ported from: JiMEditor/Views/Interactions/MultiEventWindow.xaml — Multi tab (2026-04-15).
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class MultiEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	MultiEventInteraction? _multi;

	public MultiEventCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, MultiEventInteraction multi )
	{
		_scenario = scenario;
		_multi = multi;
		TriggerSelectCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		EventSelectCombo.ItemsSource = BuildEventOptions( scenario, multi );
		SelectedTriggersItems.ItemsSource = multi.triggerList;
		SelectedEventsItems.ItemsSource = multi.eventList;
	}

	public void Hydrate()
	{
		if ( _multi == null )
			return;
		SilentCheckBox.IsChecked = _multi.isSilent;
		TriggerModeRadio.IsChecked = _multi.usingTriggers;
		EventModeRadio.IsChecked = !_multi.usingTriggers;
		SyncModeEnabled();
		// Defer once so TopLevel/BaseEventDialogWindow is available on first open.
		Dispatcher.UIThread.Post( SyncEventTextEnabled, DispatcherPriority.Loaded );
		SelectTrigger( TriggerSelectCombo, "None" );
		SelectEventName( EventSelectCombo, "None" );
	}

	internal void Commit()
	{
		if ( _multi == null )
			return;
		_multi.usingTriggers = TriggerModeRadio.IsChecked == true;
		_multi.isSilent = SilentCheckBox.IsChecked == true;
	}

	public void Detach()
	{
		_multi = null;
		_scenario = null;
	}

	public Control AsControl() => this;

	static List<string> BuildEventOptions( Scenario scenario, MultiEventInteraction current ) =>
		scenario.interactionObserver
			.Where( x => x.dataName != current.dataName && EventDialogInteractionLists.IncludeInReferenceDropdown( x ) )
			.Select( x => x.dataName )
			.Prepend( "None" )
			.Distinct()
			.OrderBy( x => x == "None" ? 0 : 1 )
			.ThenBy( x => x )
			.ToList();

	static void SelectTrigger( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not System.Collections.IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}
		var list = src.Cast<Trigger>().ToList();
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName ) ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	static void SelectEventName( ComboBox cb, string? value )
	{
		if ( cb.ItemsSource is not IEnumerable<string> src )
			return;
		cb.SelectedItem = src.FirstOrDefault( x => x == value ) ?? "None";
	}

	void SyncModeEnabled()
	{
		var usingEvents = EventModeRadio.IsChecked == true;
		var usingTriggers = !usingEvents;
		TriggerModeBox.IsEnabled = usingTriggers;
		EventModeBox.IsEnabled = usingEvents;
	}

	void SyncEventTextEnabled()
	{
		var parentWindow = TopLevel.GetTopLevel( this ) as BaseEventDialogWindow;
		parentWindow?.SetEventTextSectionEnabled( SilentCheckBox.IsChecked != true );
	}

	void ModeRadio_OnChanged( object? sender, RoutedEventArgs e ) => SyncModeEnabled();

	void SilentCheckBox_OnChanged( object? sender, RoutedEventArgs e )
	{
		if ( _multi != null )
			_multi.isSilent = SilentCheckBox.IsChecked == true;
		SyncEventTextEnabled();
	}

	void TriggerSelectCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e ) =>
		AddSelectedTriggerButton.IsEnabled = TriggerSelectCombo.SelectedItem is Trigger t && t.dataName != "None";

	void EventSelectCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e ) =>
		AddSelectedEventButton.IsEnabled = EventSelectCombo.SelectedItem is string s && s != "None";

	void AddSelectedTriggerButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _multi == null || TriggerSelectCombo.SelectedItem is not Trigger trigger || trigger.dataName == "None" )
			return;
		if ( !_multi.triggerList.Contains( trigger.dataName ) )
			_multi.triggerList.Add( trigger.dataName );
	}

	async void AddTriggerButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _scenario == null || _multi == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;
		if ( !_multi.triggerList.Contains( tw.EnteredTriggerName ) )
			_multi.triggerList.Add( tw.EnteredTriggerName );
		TriggerSelectCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
	}

	void RemoveTriggerButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _multi == null || sender is not Button button || button.CommandParameter is not string triggerName )
			return;
		if ( _multi.triggerList.Contains( triggerName ) )
			_multi.triggerList.Remove( triggerName );
	}

	void AddSelectedEventButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _multi == null || EventSelectCombo.SelectedItem is not string eventName || eventName == "None" )
			return;
		if ( !_multi.eventList.Contains( eventName ) )
			_multi.eventList.Add( eventName );
	}

	void RemoveEventButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _multi == null || sender is not Button button || button.CommandParameter is not string eventName )
			return;
		if ( _multi.eventList.Contains( eventName ) )
			_multi.eventList.Remove( eventName );
	}

	void RefreshEventOptions()
	{
		if ( _scenario == null || _multi == null )
			return;
		EventSelectCombo.ItemsSource = BuildEventOptions( _scenario, _multi );
	}

	async void AddEventButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _scenario == null || _multi == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;

		var flyout = new MenuFlyout();
		flyout.Items.Add( CreateEventMenuItem( "Dialog Event", () => EventEditorFactory.TryCreateDialogInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Text Event", () => EventEditorFactory.TryCreateTextInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Story Branch", () => EventEditorFactory.TryCreateBranchInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Reward Event", () => EventEditorFactory.TryCreateRewardInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Persistent Event", () => EventEditorFactory.TryCreatePersistentInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Conditional Event", () => EventEditorFactory.TryCreateConditionalInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Item Event", () => EventEditorFactory.TryCreateItemInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Title Event", () => EventEditorFactory.TryCreateTitleInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Corruption Event", () => EventEditorFactory.TryCreateCorruptionInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Multi-Event", () => EventEditorFactory.TryCreateMultiEventInteractionAsync( owner, _scenario ) ) );
		flyout.ShowAt( AddEventButton );
	}

	MenuItem CreateEventMenuItem<T>( string header, System.Func<System.Threading.Tasks.Task<(bool accepted, T created)>> create )
		where T : InteractionBase
	{
		var item = new MenuItem { Header = header };
		item.Click += async (_, _) =>
		{
			if ( _scenario == null || _multi == null )
				return;
			var (accepted, created) = await create();
			if ( !accepted )
				return;
			_scenario.AddInteraction( created );
			RefreshEventOptions();
			SelectEventName( EventSelectCombo, created.dataName );
		};
		return item;
	}
}
