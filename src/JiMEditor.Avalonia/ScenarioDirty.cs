// Coarse unsaved-edit tracking: collection changes + editor input controls.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JiME;

namespace JiMEditor.Avalonia;

/// <summary>
/// Marks <see cref="Scenario.isDirty"/> from collection mutations and from text/check/combo/slider edits.
/// Put <c>Classes="ignoreDirty"</c> on a control to skip it. Does not watch <see cref="ListBox"/> selection.
/// </summary>
public static class ScenarioDirty
{
	public const string IgnoreClass = "ignoreDirty";

	static readonly ConditionalWeakTable<Control, object> Watched = new();
	static readonly object WatchedSentinel = new();
	static ScenarioDirtyTracker? _active;

	public static Scenario? CurrentScenario => _active?.Scenario;

	public static ScenarioDirtyTracker Attach( Scenario scenario )
	{
		_active?.Dispose();
		var tracker = new ScenarioDirtyTracker( scenario );
		_active = tracker;
		return tracker;
	}

	internal static void ClearActive( ScenarioDirtyTracker tracker )
	{
		if ( _active == tracker )
			_active = null;
	}

	/// <summary>
	/// Subscribe to input controls after hydrate. Uses <paramref name="scenario"/> or <see cref="CurrentScenario"/>.
	/// Safe to call before <see cref="Window.Loaded"/>.
	/// Ignores the first layout/binding pass so opening a dialog (or Cancel with no edits) does not mark dirty.
	/// </summary>
	public static void WatchInputs( Window window, Scenario? scenario = null )
	{
		scenario ??= CurrentScenario;
		if ( window is null || scenario is null )
			return;

		var live = false;
		bool IsLive() => live;
		void Arm() => live = true;

		void AttachNow()
		{
			AttachTree( window, scenario, IsLive );
			Dispatcher.UIThread.Post( () => AttachTree( window, scenario, IsLive ), DispatcherPriority.Loaded );
			// ComboBox/CheckBox bindings often fire SelectionChanged/IsCheckedChanged during the first
			// layout after Loaded. Wait until the dispatcher is idle so that is not treated as an edit.
			Dispatcher.UIThread.Post(
				() => Dispatcher.UIThread.Post( Arm, DispatcherPriority.ContextIdle ),
				DispatcherPriority.ContextIdle );
		}

		if ( window.IsLoaded )
			AttachNow();
		else
		{
			void OnLoaded( object? _, RoutedEventArgs e )
			{
				window.Loaded -= OnLoaded;
				AttachNow();
			}

			window.Loaded += OnLoaded;
		}
	}

	static void AttachTree( Window window, Scenario scenario, Func<bool> isLive )
	{
		foreach ( var visual in window.GetVisualDescendants() )
		{
			if ( visual is Control control )
				TryWatch( control, scenario, isLive );
		}
	}

	static void TryWatch( Control control, Scenario scenario, Func<bool> isLive )
	{
		if ( control.Classes.Contains( IgnoreClass ) )
			return;
		try
		{
			Watched.Add( control, WatchedSentinel );
		}
		catch ( ArgumentException )
		{
			return;
		}

		void MarkIfLive()
		{
			if ( isLive() )
				scenario.MarkDirty();
		}

		switch ( control )
		{
			case TextBox { IsReadOnly: true }:
				break;
			case TextBox textBox:
				textBox.TextChanged += ( _, _ ) => MarkIfLive();
				break;
			case Slider slider:
				slider.ValueChanged += ( _, _ ) => MarkIfLive();
				break;
			case ComboBox combo:
				combo.SelectionChanged += ( _, _ ) => MarkIfLive();
				break;
			case CheckBox or RadioButton:
				( (ToggleButton)control ).IsCheckedChanged += ( _, _ ) => MarkIfLive();
				break;
		}
	}
}

/// <summary>
/// Listens to scenario collections (including nested tiles/tokens). Dispose when the editor window closes.
/// </summary>
public sealed class ScenarioDirtyTracker : IDisposable
{
	readonly NotifyCollectionChangedEventHandler _markDirty;
	readonly NotifyCollectionChangedEventHandler _chaptersChanged;
	readonly Dictionary<Chapter, NotifyCollectionChangedEventHandler> _tileHandlers = new();
	readonly Dictionary<BaseTile, NotifyCollectionChangedEventHandler> _tokenHandlers = new();
	bool _disposed;

	public Scenario Scenario { get; }

	internal ScenarioDirtyTracker( Scenario scenario )
	{
		Scenario = scenario;
		_markDirty = ( _, _ ) => scenario.MarkDirty();
		_chaptersChanged = OnChaptersChanged;

		scenario.interactionObserver.CollectionChanged += _markDirty;
		scenario.triggersObserver.CollectionChanged += _markDirty;
		scenario.objectiveObserver.CollectionChanged += _markDirty;
		scenario.monsterModifierObserver.CollectionChanged += _markDirty;
		scenario.activationsObserver.CollectionChanged += _markDirty;
		scenario.resolutionObserver.CollectionChanged += _markDirty;
		scenario.threatObserver.CollectionChanged += _markDirty;
		scenario.translationObserver.CollectionChanged += _markDirty;
		scenario.collectionObserver.CollectionChanged += _markDirty;
		scenario.globalTilePool.CollectionChanged += _markDirty;
		scenario.chapterObserver.CollectionChanged += _chaptersChanged;

		foreach ( var chapter in scenario.chapterObserver )
			AttachChapter( chapter );
	}

	void OnChaptersChanged( object? sender, NotifyCollectionChangedEventArgs e )
	{
		Scenario.MarkDirty();
		if ( e.Action == NotifyCollectionChangedAction.Reset )
		{
			DetachAllNested();
			foreach ( var chapter in Scenario.chapterObserver )
				AttachChapter( chapter );
			return;
		}

		if ( e.OldItems != null )
		{
			foreach ( var item in e.OldItems )
			{
				if ( item is Chapter chapter )
					DetachChapter( chapter );
			}
		}

		if ( e.NewItems != null )
		{
			foreach ( var item in e.NewItems )
			{
				if ( item is Chapter chapter )
					AttachChapter( chapter );
			}
		}
	}

	void AttachChapter( Chapter chapter )
	{
		if ( _tileHandlers.ContainsKey( chapter ) )
			return;

		void OnTilesChanged( object? sender, NotifyCollectionChangedEventArgs e )
		{
			Scenario.MarkDirty();
			SyncTiles( e );
		}

		_tileHandlers[chapter] = OnTilesChanged;
		chapter.tileObserver.CollectionChanged += OnTilesChanged;
		foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
			AttachTile( tile );
	}

	void DetachChapter( Chapter chapter )
	{
		if ( _tileHandlers.TryGetValue( chapter, out var handler ) )
		{
			chapter.tileObserver.CollectionChanged -= handler;
			_tileHandlers.Remove( chapter );
		}

		foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
			DetachTile( tile );
	}

	void SyncTiles( NotifyCollectionChangedEventArgs e )
	{
		if ( e.Action == NotifyCollectionChangedAction.Reset )
			return;

		if ( e.OldItems != null )
		{
			foreach ( var item in e.OldItems )
			{
				if ( item is BaseTile tile )
					DetachTile( tile );
			}
		}

		if ( e.NewItems != null )
		{
			foreach ( var item in e.NewItems )
			{
				if ( item is BaseTile tile )
					AttachTile( tile );
			}
		}
	}

	void AttachTile( BaseTile tile )
	{
		if ( _tokenHandlers.ContainsKey( tile ) )
			return;

		void OnTokensChanged( object? _, NotifyCollectionChangedEventArgs e ) => Scenario.MarkDirty();
		_tokenHandlers[tile] = OnTokensChanged;
		tile.tokenList.CollectionChanged += OnTokensChanged;
	}

	void DetachTile( BaseTile tile )
	{
		if ( !_tokenHandlers.TryGetValue( tile, out var handler ) )
			return;
		tile.tokenList.CollectionChanged -= handler;
		_tokenHandlers.Remove( tile );
	}

	void DetachAllNested()
	{
		foreach ( var pair in _tileHandlers )
			pair.Key.tileObserver.CollectionChanged -= pair.Value;
		_tileHandlers.Clear();
		foreach ( var pair in _tokenHandlers )
			pair.Key.tokenList.CollectionChanged -= pair.Value;
		_tokenHandlers.Clear();
	}

	public void Dispose()
	{
		if ( _disposed )
			return;
		_disposed = true;
		ScenarioDirty.ClearActive( this );

		Scenario.interactionObserver.CollectionChanged -= _markDirty;
		Scenario.triggersObserver.CollectionChanged -= _markDirty;
		Scenario.objectiveObserver.CollectionChanged -= _markDirty;
		Scenario.monsterModifierObserver.CollectionChanged -= _markDirty;
		Scenario.activationsObserver.CollectionChanged -= _markDirty;
		Scenario.resolutionObserver.CollectionChanged -= _markDirty;
		Scenario.threatObserver.CollectionChanged -= _markDirty;
		Scenario.translationObserver.CollectionChanged -= _markDirty;
		Scenario.collectionObserver.CollectionChanged -= _markDirty;
		Scenario.globalTilePool.CollectionChanged -= _markDirty;
		Scenario.chapterObserver.CollectionChanged -= _chaptersChanged;
		DetachAllNested();
	}
}
