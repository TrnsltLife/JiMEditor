// Session-only FlowGraph preview wired through JiM scenario graph abstractions. See docs/ScenarioGraphAvaloniaPlan.md.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FlowGraph.Core;
using JiMEditor.Avalonia.ScenarioGraph;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class ScenarioChapterFlowGraphView : UserControl
{
	static readonly TimeSpan RebuildDebounce = TimeSpan.FromMilliseconds( 85 );

	Scenario? _scenario;
	readonly LayeredRelaxationLayoutEngine _layeredLayoutEngine = new();
	readonly WpfTreeLayoutEngine _wpfTreeLayoutEngine = new();
	ScenarioGraphLayoutMode _layoutMode = ScenarioGraphLayoutMode.WpfTree;
	EdgeType? _forcedEdgeType = EdgeType.Bezier;
	IScenarioGraphViewHost? _graphHost;
	IScenarioGraphInteractionSink? _interactionSink;
	NotifyCollectionChangedEventHandler? _collectionsChangedHandler;
	readonly List<(INotifyCollectionChanged Collection, NotifyCollectionChangedEventHandler Handler)> _nestedSubscriptions = new();
	DispatcherTimer? _rebuildDebounceTimer;
	int _graphFitGeneration;

	public ScenarioChapterFlowGraphView()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		FlowDotsBackground.TargetCanvas = FlowHostCanvas;
		ApplyInteractionDefaults();
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

	/// <summary>
	/// If the current graph contains a node with <paramref name="nodeId"/>, selects it, clears other selection, and pans/zooms to center it (WPF sidebar → graph parity).
	/// </summary>
	public void TryCenterOnGraphNodeId( string nodeId )
	{
		if ( string.IsNullOrEmpty( nodeId ) )
			return;
		var graph = FlowHostCanvas.Graph;
		if ( graph == null )
			return;
		var node = graph.Elements.Nodes.FirstOrDefault( n => n.Id == nodeId );
		if ( node == null )
			return;

		// Match canvas click semantics: single node selected (and no selected edges).
		FlowHostCanvas.Selection.DeselectAll();
		foreach ( var edge in graph.Elements.Edges.Where( ed => ed.IsSelected ) )
			edge.IsSelected = false;
		foreach ( var n in graph.Elements.Nodes.Where( x => x.IsSelected ) )
			n.IsSelected = false;
		node.IsSelected = true;
		FlowHostCanvas.Selection.NotifySelectionMayHaveChanged();

		FlowHostCanvas.CenterOnNodeAnimated( node, 0.35, null );
	}

	void RefreshGraphButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		RefreshFromScenario();

	void TreeLayoutButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e )
	{
		_layoutMode = ScenarioGraphLayoutMode.WpfTree;
		RefreshGraphNow();
	}

	void LayeredLayoutButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e )
	{
		_layoutMode = ScenarioGraphLayoutMode.LayeredRelaxation;
		RefreshGraphNow();
	}

	void SmoothStepEdgesButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		ApplyGlobalEdgeType( EdgeType.SmoothStep );

	void StepEdgesButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		ApplyGlobalEdgeType( EdgeType.Step );

	void BezierEdgesButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		ApplyGlobalEdgeType( EdgeType.Bezier );

	void StraightEdgesButton_Click( object? sender, global::Avalonia.Interactivity.RoutedEventArgs e ) =>
		ApplyGlobalEdgeType( EdgeType.Straight );

	void ApplyGlobalEdgeType( EdgeType edgeType )
	{
		_forcedEdgeType = edgeType;
		RefreshGraphNow();
	}

	IScenarioGraphLayoutEngine ActiveLayoutEngine =>
		_layoutMode switch
		{
			ScenarioGraphLayoutMode.WpfTree => _wpfTreeLayoutEngine,
			_ => _layeredLayoutEngine,
		};

	void RefreshGraphNow()
	{
		if ( _scenario == null || _graphHost == null )
			return;

		UnwireNestedScenarioCollections();
		WireNestedScenarioCollections();

		var document = ScenarioGraphDocumentBuilder.Build( _scenario, ScenarioGraphBuildScope.FullScenario );
		// Keep visual-tree mode so custom polygon renderers (hex/square) apply consistently.
		// NOTE: do not call DisableSimplifiedRendering() here; it can reset node renderers to defaults.
		// FlowGraph auto-enables direct rendering when node count >= DirectRenderingNodeThreshold (default 100).
		// Direct mode bypasses the visual tree: custom NodeRenderers and non-straight edge paths do not apply.
		ApplyInteractionDefaults();
		FlowHostCanvas.DisableDirectRendering();

		FlowHostCanvas.StopAllAnimations();
		unchecked { _graphFitGeneration++; }
		var fitGen = _graphFitGeneration;

		var layout = ActiveLayoutEngine.Layout( document );
		layout.ForcedEdgeType = _forcedEdgeType;
		_graphHost.Present( document, layout );

		Dispatcher.UIThread.Post(
			() =>
			{
				if ( fitGen != _graphFitGeneration )
					return;
				FlowHostCanvas.FitToViewAnimated( 0.25 );
			},
			DispatcherPriority.Loaded );
	}

	void ApplyInteractionDefaults()
	{
		FlowHostCanvas.Settings.IsReadOnly = false;
		// Scenario graph is read-mostly: keep edge hit area tiny so node click/drag wins.
		FlowHostCanvas.Settings.EdgeHitAreaWidth = 2;
		FlowHostCanvas.Settings.EnableEdgeLabelEditing = false;
		FlowHostCanvas.Settings.EnableNodeLabelEditing = true;
		// 0 = never auto-switch to direct rendering by node count (see FlowCanvas.RenderElements).
		FlowHostCanvas.Settings.DirectRenderingNodeThreshold = 0;

		// Edges: FlowGraph virtualizes by testing node bounds vs viewport only. Long edges whose segment
		// crosses the view but whose endpoints are off-screen would otherwise disappear; disable for this graph.
		FlowHostCanvas.Settings.EnableVirtualization = false;
		// Allow zooming out further so the full scenario stays navigable; edges stay logical-thickness under transform.
		FlowHostCanvas.Settings.MinZoom = 0.03;
		// Slightly thicker strokes so links remain readable at low zoom (still scaled by canvas transform).
		FlowHostCanvas.Settings.EdgeStrokeThickness = 2.5;
		FlowHostCanvas.Settings.EdgeSelectedStrokeThickness = 3.5;
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

		FlowHostCanvas.NodeRenderers.Register( "event-rect", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Rectangle,
			fillHex: "#6B4E7A",
			strokeHex: "#D1B7DE",
			fontSize: 13,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "resolution-rect", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.Rectangle,
			fillHex: "#3A6A70",
			strokeHex: "#9DD9E2",
			fontSize: 13,
			fontWeight: FontWeight.SemiBold ) );

		FlowHostCanvas.NodeRenderers.Register( "threat-node", new ShapeNodeRenderer(
			shape: ShapeNodeRenderer.NodeShape.EquilateralTriangleUp,
			fillHex: "#7A3F3F",
			strokeHex: "#E7A4A4",
			fontSize: 13,
			fontWeight: FontWeight.SemiBold ) );
	}
}
