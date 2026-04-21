using FlowGraph.Avalonia;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiMEditor.Avalonia.ScenarioGraph;

public sealed class FlowGraphScenarioGraphViewHost : IScenarioGraphViewHost
{
	readonly FlowCanvas _canvas;
	IScenarioGraphInteractionSink? _sink;
	readonly Dictionary<string, ScenarioGraphNode> _nodesById = new();

	public FlowGraphScenarioGraphViewHost( FlowCanvas canvas )
	{
		_canvas = canvas;
		_canvas.NodeLabelEditRequested += OnNodeLabelEditRequested;
	}

	public void Present( IScenarioGraphDocument document, ScenarioGraphLayoutResult layout )
	{
		_nodesById.Clear();
		foreach ( var node in document.Nodes )
			_nodesById[node.Id] = node;
		_canvas.Graph = FlowGraphScenarioDocumentMapper.ToFlowGraph( document, layout );
	}

	public void SetInteractionSink( IScenarioGraphInteractionSink? sink ) => _sink = sink;

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
