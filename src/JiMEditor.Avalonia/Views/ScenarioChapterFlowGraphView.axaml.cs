// Session-only FlowGraph preview wired through JiM scenario graph abstractions. See docs/ScenarioGraphAvaloniaPlan.md.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using JiMEditor.Avalonia.ScenarioGraph;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class ScenarioChapterFlowGraphView : UserControl
{
	static readonly TimeSpan RebuildDebounce = TimeSpan.FromMilliseconds( 85 );

	Scenario? _scenario;
	readonly IScenarioGraphLayoutEngine _layoutEngine = new LayeredRelaxationLayoutEngine();
	IScenarioGraphViewHost? _graphHost;
	IScenarioGraphInteractionSink? _interactionSink;
	NotifyCollectionChangedEventHandler? _collectionsChangedHandler;
	readonly List<(INotifyCollectionChanged Collection, NotifyCollectionChangedEventHandler Handler)> _nestedSubscriptions = new();
	DispatcherTimer? _rebuildDebounceTimer;

	public ScenarioChapterFlowGraphView()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		FlowDotsBackground.TargetCanvas = FlowHostCanvas;
		_graphHost = new FlowGraphScenarioGraphViewHost( FlowHostCanvas );
		_graphHost.SetInteractionSink( _interactionSink );
		RegisterCustomNodeRenderers();
		if ( _scenario != null )
			RefreshGraphNow();
	}

	public void SetInteractionSink( IScenarioGraphInteractionSink? sink )
	{
		_interactionSink = sink;
		_graphHost?.SetInteractionSink( sink );
	}

	public void Attach( Scenario scenario )
	{
		Detach();
		_scenario = scenario;
		_collectionsChangedHandler = OnScenarioCollectionChanged;
		scenario.chapterObserver.CollectionChanged += _collectionsChangedHandler;
		scenario.triggersObserver.CollectionChanged += _collectionsChangedHandler;
		scenario.objectiveObserver.CollectionChanged += _collectionsChangedHandler;
		scenario.interactionObserver.CollectionChanged += _collectionsChangedHandler;
		scenario.resolutionObserver.CollectionChanged += _collectionsChangedHandler;
		scenario.threatObserver.CollectionChanged += _collectionsChangedHandler;
		RefreshGraphNow();
	}

	public void Detach()
	{
		if ( _rebuildDebounceTimer != null )
		{
			_rebuildDebounceTimer.Tick -= OnRebuildDebounceTick;
			_rebuildDebounceTimer.Stop();
			_rebuildDebounceTimer = null;
		}

		UnwireNestedScenarioCollections();

		if ( _scenario != null && _collectionsChangedHandler != null )
		{
			_scenario.chapterObserver.CollectionChanged -= _collectionsChangedHandler;
			_scenario.triggersObserver.CollectionChanged -= _collectionsChangedHandler;
			_scenario.objectiveObserver.CollectionChanged -= _collectionsChangedHandler;
			_scenario.interactionObserver.CollectionChanged -= _collectionsChangedHandler;
			_scenario.resolutionObserver.CollectionChanged -= _collectionsChangedHandler;
			_scenario.threatObserver.CollectionChanged -= _collectionsChangedHandler;
		}

		_collectionsChangedHandler = null;
		_scenario = null;
	}

	void OnScenarioCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) =>
		ScheduleGraphRebuild();

	void OnNestedCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) =>
		ScheduleGraphRebuild();

	void ScheduleGraphRebuild()
	{
		if ( _scenario == null )
			return;

		_rebuildDebounceTimer ??= new DispatcherTimer { Interval = RebuildDebounce };
		_rebuildDebounceTimer.Tick -= OnRebuildDebounceTick;
		_rebuildDebounceTimer.Tick += OnRebuildDebounceTick;
		_rebuildDebounceTimer.Stop();
		_rebuildDebounceTimer.Start();
	}

	void OnRebuildDebounceTick( object? sender, EventArgs e )
	{
		_rebuildDebounceTimer?.Stop();
		RefreshGraphNow();
	}

	public void RefreshFromScenario() => RefreshGraphNow();

	void RefreshGraphButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		RefreshFromScenario();

	void RefreshGraphNow()
	{
		if ( _scenario == null || _graphHost == null )
			return;

		UnwireNestedScenarioCollections();
		WireNestedScenarioCollections();

		var document = ScenarioGraphDocumentBuilder.Build( _scenario, ScenarioGraphBuildScope.FullScenario );
		// Keep visual-tree mode so custom polygon renderers (hex/square) apply consistently.
		// NOTE: do not call DisableSimplifiedRendering() here; it can reset node renderers to defaults.
		FlowHostCanvas.DisableDirectRendering();

		var layout = _layoutEngine.Layout( document );
		_graphHost.Present( document, layout );

		Dispatcher.UIThread.Post(
			() => FlowHostCanvas.FitToViewAnimated( 0.25 ),
			DispatcherPriority.Loaded );
	}

	void WireNestedScenarioCollections()
	{
		if ( _scenario == null )
			return;

		foreach ( var chapter in _scenario.chapterObserver )
		{
			chapter.tileObserver.CollectionChanged += OnNestedCollectionChanged;
			_nestedSubscriptions.Add( (chapter.tileObserver, OnNestedCollectionChanged ) );
			foreach ( var tile in chapter.tileObserver )
			{
				if ( tile is BaseTile bt )
				{
					bt.tokenList.CollectionChanged += OnNestedCollectionChanged;
					_nestedSubscriptions.Add( (bt.tokenList, OnNestedCollectionChanged ) );
				}
			}
		}
	}

	void UnwireNestedScenarioCollections()
	{
		foreach ( var (collection, handler) in _nestedSubscriptions )
			collection.CollectionChanged -= handler;
		_nestedSubscriptions.Clear();
	}

	void RegisterCustomNodeRenderers()
	{
		FlowHostCanvas.NodeRenderers.Register( "chapter-hex", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Hexagon,
			fillHex: "#4A6172",
			strokeHex: "#A9C7DA",
			fontSize: 14,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "chapter-square", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Square,
			fillHex: "#455B45",
			strokeHex: "#A9D8A9" ) );

		FlowHostCanvas.NodeRenderers.Register( "tile-hex", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Hexagon,
			fillHex: "#5A4E75",
			strokeHex: "#C5B7E5",
			fontSize: 14,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "tile-square", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Square,
			fillHex: "#6A5A3B",
			strokeHex: "#E0D2A6" ) );

		FlowHostCanvas.NodeRenderers.Register( "token-circle", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Circle,
			fillHex: "#3A5A7A",
			strokeHex: "#A8C9E8",
			fontSize: 14,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "token-diamond", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Diamond,
			fillHex: "#6C4A28",
			strokeHex: "#E6C39D",
			fontSize: 14,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "trigger-arrow", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.TriggerArrow,
			fillHex: "#7A3F3F",
			strokeHex: "#E7A4A4",
			fontSize: 14,
			fontWeight: FontWeight.SemiBold ) );
	}
}
