// Ported from: JiMEditor/Views/Interactions/BranchInteractionWindow.xaml.cs — Story Branch tab (2026-04-18).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JiMEditor.Avalonia.EventDialog;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class BranchEventCustomTab : UserControl, IEventCustomTab
{
	bool _suppress;
	Scenario? _scenario;
	BranchInteraction? _branch;

	public BranchEventCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, BranchInteraction branch )
	{
		_scenario = scenario;
		_branch = branch;
		TriggerTestCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		TriggerTrueCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		TriggerFalseCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		RefreshEventCombos();
	}

	public void Detach()
	{
	}

	public Control AsControl() => this;

	public void Hydrate()
	{
		if ( _branch == null )
			return;
		_suppress = true;
		try
		{
			SelectTrigger( TriggerTestCombo, _branch.triggerTest );
			SelectTrigger( TriggerTrueCombo, _branch.triggerIsSetTrigger );
			SelectTrigger( TriggerFalseCombo, _branch.triggerNotSetTrigger );
			SelectString( EventTrueCombo, _branch.triggerIsSet );
			SelectString( EventFalseCombo, _branch.triggerNotSet );
			EventBranchRadio.IsChecked = _branch.branchTestEvent;
			TriggerBranchRadio.IsChecked = !_branch.branchTestEvent;
			SyncModeEnabled();
		}
		finally
		{
			_suppress = false;
		}
	}

	internal void Commit()
	{
		if ( _branch == null )
			return;
		_branch.triggerTest = TriggerTestCombo.SelectedItem is Trigger t1 ? t1.dataName : "None";
		_branch.triggerIsSet = EventTrueCombo.SelectedItem as string ?? "None";
		_branch.triggerNotSet = EventFalseCombo.SelectedItem as string ?? "None";
		_branch.triggerIsSetTrigger = TriggerTrueCombo.SelectedItem is Trigger t2 ? t2.dataName : "None";
		_branch.triggerNotSetTrigger = TriggerFalseCombo.SelectedItem is Trigger t3 ? t3.dataName : "None";
		_branch.branchTestEvent = EventBranchRadio.IsChecked == true;
	}

	void SyncModeEnabled()
	{
		var eventOn = EventBranchRadio.IsChecked == true;
		var triggerOn = TriggerBranchRadio.IsChecked == true;
		// During group switches Avalonia can briefly report both false; never leave both panes disabled.
		if ( eventOn && !triggerOn )
		{
			EventModeBox.IsEnabled = true;
			TriggerModeBox.IsEnabled = false;
		}
		else if ( !eventOn && triggerOn )
		{
			EventModeBox.IsEnabled = false;
			TriggerModeBox.IsEnabled = true;
		}
		else if ( eventOn && triggerOn )
		{
			EventModeBox.IsEnabled = true;
			TriggerModeBox.IsEnabled = false;
		}
		else
		{
			EventModeBox.IsEnabled = true;
			TriggerModeBox.IsEnabled = false;
		}
	}

	void BranchModeRadio_IsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress )
			return;
		// After mutual exclusion updates, IsChecked is stable (avoids both panels disabled mid-toggle).
		Dispatcher.UIThread.Post( SyncModeFromRadios, DispatcherPriority.Input );
	}

	void SyncModeFromRadios()
	{
		if ( _branch == null )
			return;
		if ( EventBranchRadio.IsChecked == true )
			_branch.branchTestEvent = true;
		else if ( TriggerBranchRadio.IsChecked == true )
			_branch.branchTestEvent = false;
		SyncModeEnabled();
	}

	void TriggerTestCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppress )
			return;
		if ( TriggerTestCombo.SelectedItem is Trigger t && t.dataName == "Random Event" )
		{
			_suppress = true;
			try
			{
				SelectTrigger( TriggerTestCombo, "None" );
			}
			finally
			{
				_suppress = false;
			}
		}
	}

	static void SelectTrigger( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}

		var list = src.Cast<Trigger>().ToList();
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName )
		                   ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	static void SelectString( ComboBox cb, string? value )
	{
		if ( cb.ItemsSource is not IEnumerable<string> src )
			return;
		var list = src.ToList();
		cb.SelectedItem = list.FirstOrDefault( x => x == value ) ?? "None";
	}

	void RefreshEventCombos()
	{
		if ( _scenario == null || _branch == null )
			return;
		var items = BuildEventNames().ToArray();
		var keepTrue = EventTrueCombo.SelectedItem as string;
		var keepFalse = EventFalseCombo.SelectedItem as string;
		EventTrueCombo.ItemsSource = items;
		EventFalseCombo.ItemsSource = items;
		SelectString( EventTrueCombo, keepTrue );
		SelectString( EventFalseCombo, keepFalse );
	}

	IEnumerable<string> BuildEventNames()
	{
		if ( _scenario == null || _branch == null )
			yield break;
		yield return "None";
		foreach ( var name in _scenario.interactionObserver
			         .Where( i => EventDialogInteractionLists.IncludeInReferenceDropdown( i ) )
			         .Select( i => i.dataName )
			         .Where( n =>
				         n != _branch.dataName
				         && !string.Equals( n, "None", StringComparison.OrdinalIgnoreCase ) )
			         .Distinct( StringComparer.OrdinalIgnoreCase )
			         .OrderBy( n => n, StringComparer.OrdinalIgnoreCase ) )
			yield return name;
	}

	async void AddTriggerTestButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( TriggerTestCombo );

	async void AddTriggerTrueButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( TriggerTrueCombo );

	async void AddTriggerFalseButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await AddTriggerAndSelectAsync( TriggerFalseCombo );

	async Task AddTriggerAndSelectAsync( ComboBox target )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;
		var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver ).ToList();
		_suppress = true;
		try
		{
			TriggerTestCombo.ItemsSource = sorted;
			TriggerTrueCombo.ItemsSource = sorted;
			TriggerFalseCombo.ItemsSource = sorted;
			SelectTrigger( target, tw.EnteredTriggerName );
		}
		finally
		{
			_suppress = false;
		}
	}

	void AddEventTrueButton_OnClick( object? sender, RoutedEventArgs e ) =>
		ShowNewEventFlyoutAt( AddEventTrueButton, EventTrueCombo );

	void AddEventFalseButton_OnClick( object? sender, RoutedEventArgs e ) =>
		ShowNewEventFlyoutAt( AddEventFalseButton, EventFalseCombo );

	void ShowNewEventFlyoutAt( Control anchor, ComboBox targetEventCombo )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var flyout = new MenuFlyout();
		flyout.Items.Add( CreateEventMenuItem( "Dialog Event", targetEventCombo, () => EventEditorFactory.TryCreateDialogInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Text Event", targetEventCombo, () => EventEditorFactory.TryCreateTextInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Reward Event", targetEventCombo, () => EventEditorFactory.TryCreateRewardInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Persistent Event", targetEventCombo, () => EventEditorFactory.TryCreatePersistentInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Conditional Event", targetEventCombo, () => EventEditorFactory.TryCreateConditionalInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Item Event", targetEventCombo, () => EventEditorFactory.TryCreateItemInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Title Event", targetEventCombo, () => EventEditorFactory.TryCreateTitleInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Corruption Event", targetEventCombo, () => EventEditorFactory.TryCreateCorruptionInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Decision Event", targetEventCombo, () => EventEditorFactory.TryCreateDecisionInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Multi-Event", targetEventCombo, () => EventEditorFactory.TryCreateMultiEventInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Enemy Event", targetEventCombo, () => EventEditorFactory.TryCreateThreatInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Story Branch", targetEventCombo, () => EventEditorFactory.TryCreateBranchInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Stat Test Event", targetEventCombo, () => EventEditorFactory.TryCreateStatTestInteractionAsync( owner, _scenario ) ) );
		flyout.ShowAt( anchor );
	}

	MenuItem CreateEventMenuItem<T>( string header, ComboBox targetEventCombo, Func<Task<(bool accepted, T created)>> create )
		where T : InteractionBase
	{
		var item = new MenuItem { Header = header };
		item.Click += async (_, _) =>
		{
			if ( _scenario == null )
				return;
			var (accepted, created) = await create();
			if ( !accepted )
				return;
			_scenario.AddInteraction( created );
			RefreshEventCombos();
			SelectString( targetEventCombo, created.dataName );
		};
		return item;
	}
}
