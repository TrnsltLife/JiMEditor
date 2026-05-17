using Avalonia.Threading;
using FlowGraph.Avalonia;
using FlowGraph.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiMEditor.Avalonia.ScenarioGraph;

public sealed class FlowGraphScenarioGraphViewHost : IScenarioGraphViewHost
{
	readonly FlowCanvas _canvas;
	IScenarioGraphInteractionSink? _sink;
	readonly Dictionary<string, ScenarioGraphNode> _nodesById = new();
	int _presentGeneration;

	public FlowGraphScenarioGraphViewHost( FlowCanvas canvas )
	{
		_canvas = canvas;
		_canvas.NodeLabelEditRequested += OnNodeLabelEditRequested;
	}

	public void Present( IScenarioGraphDocument document, ScenarioGraphLayoutResult layout )
	{
		unchecked { _presentGeneration++; }
		var gen = _presentGeneration;

		_nodesById.Clear();
		foreach ( var node in document.Nodes )
			_nodesById[node.Id] = node;

		_canvas.StopAllAnimations();
		_canvas.Selection.DeselectAll();

		var nextGraph = FlowGraphScenarioDocumentMapper.ToFlowGraph( document, layout );
		// Assign new graph immediately so input dispatcher, selection manager, and hit tester
		// all bind to the same model in a single UI turn.
		var omitEdges = layout.OmitEdges;
		var topDown = layout.UseTopDownFlowPorts;
		var layeredSecondPass = !topDown;
		_canvas.Graph = nextGraph;
		PostDeferredRefresh( gen );
		if ( omitEdges )
			return;

		_canvas.Routing.ClearAllRoutes( forceRefresh: true );
		if ( topDown )
			RouteAllEdgesTopDown();
		else
			_canvas.Routing.RouteAllEdges( forceRefresh: true );

		Dispatcher.UIThread.Post(
			() =>
			{
				if ( gen != _presentGeneration )
					return;
				if ( layeredSecondPass )
					_canvas.Routing.RouteAllEdges( forceRefresh: true );
				else if ( topDown )
					RouteAllEdgesTopDown();
				PostDeferredRefresh( gen );
			},
			DispatcherPriority.Loaded );
	}

	public void SetInteractionSink( IScenarioGraphInteractionSink? sink ) => _sink = sink;

	void RouteAllEdgesTopDown()
	{
		var savedAuto = _canvas.Settings.AutoRouteEdges;
		var savedRouter = _canvas.Settings.DefaultRouterAlgorithm;
		try
		{
			_canvas.Settings.AutoRouteEdges = false;
			_canvas.Settings.DefaultRouterAlgorithm = FlowGraph.Avalonia.RouterAlgorithm.Direct;
			_canvas.Routing.RouteAllEdges( forceRefresh: true );
		}
		finally
		{
			_canvas.Settings.AutoRouteEdges = savedAuto;
			_canvas.Settings.DefaultRouterAlgorithm = savedRouter;
		}
	}

	void PostDeferredRefresh( int generation )
	{
		// FlowCanvasContextMenu uses the same Render-priority refresh after AddNode.
		// We mirror that here after layout swaps so selection/drag state is fully active immediately.
		Dispatcher.UIThread.Post(
			() =>
			{
				if ( generation != _presentGeneration )
					return;
				_canvas.Refresh();
			},
			DispatcherPriority.Render );
	}

	async void OnNodeLabelEditRequested( object? sender, NodeLabelEditRequestedEventArgs e )
	{
		if ( _sink == null )
			return;
		if ( !_nodesById.TryGetValue( e.Node.Id, out var node ) )
			return;
		e.Handled = true;
		await _sink.OnNodeActivatedAsync( node );
	}
}
