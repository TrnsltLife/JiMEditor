// Ported from: JiMEditor/Views/Interactions/ReplaceTokenInteractionWindow.xaml.cs — Replace Token tab (2026-04-17).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class ReplaceTokenCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	ReplaceTokenInteraction? _replace;
	NotifyCollectionChangedEventHandler? _collectionHandler;
	PropertyChangedEventHandler? _replacePropHandler;

	public ReplaceTokenCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, ReplaceTokenInteraction replace )
	{
		Detach();
		_scenario = scenario;
		_replace = replace;
		_collectionHandler = OnInteractionObserverCollectionChanged;
		scenario.interactionObserver.CollectionChanged += _collectionHandler;
		if ( replace is INotifyPropertyChanged inpc )
		{
			_replacePropHandler = OnReplacePropertyChanged;
			inpc.PropertyChanged += _replacePropHandler;
		}

		RebuildEligibleLists();
	}

	public void Hydrate()
	{
		if ( _replace == null )
			return;
		NoTextCheckBox.IsChecked = _replace.noText;
		SelectInteraction( EventToReplaceCombo, _replace.eventToReplace );
		SelectInteraction( ReplaceWithCombo, _replace.replaceWithEvent );
		UpdateNoTextSectionVisibility();
	}

	internal void Commit()
	{
		if ( _replace == null )
			return;
		_replace.noText = NoTextCheckBox.IsChecked == true;
		_replace.eventToReplace = ( EventToReplaceCombo.SelectedItem as IInteraction )?.dataName ?? "None";
		_replace.replaceWithEvent = ( ReplaceWithCombo.SelectedItem as IInteraction )?.dataName ?? "None";
	}

	public void Detach()
	{
		if ( _scenario != null && _collectionHandler != null )
			_scenario.interactionObserver.CollectionChanged -= _collectionHandler;
		_collectionHandler = null;
		if ( _replace is INotifyPropertyChanged inpc && _replacePropHandler != null )
			inpc.PropertyChanged -= _replacePropHandler;
		_replacePropHandler = null;
		_replace = null;
		_scenario = null;
	}

	public Control AsControl() => this;

	void OnInteractionObserverCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		if ( e.Action is NotifyCollectionChangedAction.Add
		     or NotifyCollectionChangedAction.Remove
		     or NotifyCollectionChangedAction.Replace
		     or NotifyCollectionChangedAction.Move
		     or NotifyCollectionChangedAction.Reset )
		{
			var keepTarget = _replace?.eventToReplace;
			var keepSource = _replace?.replaceWithEvent;
			RebuildEligibleLists();
			SelectInteraction( EventToReplaceCombo, keepTarget );
			SelectInteraction( ReplaceWithCombo, keepSource );
		}
	}

	void OnReplacePropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName == nameof( InteractionBase.isTokenInteraction ) )
			UpdateNoTextSectionVisibility();
	}

	void UpdateNoTextSectionVisibility()
	{
		// WPF: visible when Token Interaction is NOT checked (binvVis on isTokenCB).
		NoTextBorder.IsVisible = !( _replace?.isTokenInteraction ?? true );
	}

	void RebuildEligibleLists()
	{
		if ( _scenario == null || _replace == null )
			return;
		var list = BuildEligibleInteractions().ToList();
		EventToReplaceCombo.ItemsSource = list;
		ReplaceWithCombo.ItemsSource = list;
	}

	IEnumerable<IInteraction> BuildEligibleInteractions()
	{
		if ( _scenario == null || _replace == null )
			yield break;
		foreach ( var x in _scenario.interactionObserver
			         .Where( x =>
				         EventDialogInteractionLists.IncludeInReferenceDropdown( x )
				         && ( x.isTokenInteraction || x.dataName == "None" )
				         && x.dataName != _replace.dataName
				         && x.dataName?.Contains( "GRP", StringComparison.Ordinal ) != true )
			         .OrderByDescending( x => x is InteractionBase { isEmpty: true } )
			         .ThenBy( x => x.dataName, StringComparer.OrdinalIgnoreCase ) )
			yield return x;
	}

	static void SelectInteraction( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not System.Collections.IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}

		var list = src.Cast<IInteraction>().ToList();
		cb.SelectedItem = list.FirstOrDefault( x => x.dataName == dataName )
		                  ?? list.FirstOrDefault( x => x.dataName == "None" );
	}

	async void AddEventToReplaceButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await ShowNewEventFlyoutAtAsync( AddEventToReplaceButton, EventToReplaceCombo );

	async void AddReplaceWithButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await ShowNewEventFlyoutAtAsync( AddReplaceWithButton, ReplaceWithCombo );

	async Task ShowNewEventFlyoutAtAsync( Control anchor, ComboBox targetCombo )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var flyout = new MenuFlyout();
		flyout.Items.Add( CreateEventMenuItem( "Dialog Event", targetCombo, () => EventEditorFactory.TryCreateDialogInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Text Event", targetCombo, () => EventEditorFactory.TryCreateTextInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Reward Event", targetCombo, () => EventEditorFactory.TryCreateRewardInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Persistent Event", targetCombo, () => EventEditorFactory.TryCreatePersistentInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Conditional Event", targetCombo, () => EventEditorFactory.TryCreateConditionalInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Item Event", targetCombo, () => EventEditorFactory.TryCreateItemInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Title Event", targetCombo, () => EventEditorFactory.TryCreateTitleInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Corruption Event", targetCombo, () => EventEditorFactory.TryCreateCorruptionInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Decision Event", targetCombo, () => EventEditorFactory.TryCreateDecisionInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Multi-Event", targetCombo, () => EventEditorFactory.TryCreateMultiEventInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Enemy Event", targetCombo, () => EventEditorFactory.TryCreateThreatInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Story Branch", targetCombo, () => EventEditorFactory.TryCreateBranchInteractionAsync( owner, _scenario ) ) );
		flyout.Items.Add( CreateEventMenuItem( "Replace Token Event", targetCombo, () => EventEditorFactory.TryCreateReplaceTokenInteractionAsync( owner, _scenario ) ) );
		flyout.ShowAt( anchor );
	}

	MenuItem CreateEventMenuItem<T>( string header, ComboBox targetCombo, Func<Task<(bool accepted, T created)>> create )
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
			var keepTarget = _replace?.eventToReplace;
			var keepSource = _replace?.replaceWithEvent;
			RebuildEligibleLists();
			SelectInteraction( EventToReplaceCombo, keepTarget );
			SelectInteraction( ReplaceWithCombo, keepSource );
			SelectInteraction( targetCombo, created.dataName );
		};
		return item;
	}
}
