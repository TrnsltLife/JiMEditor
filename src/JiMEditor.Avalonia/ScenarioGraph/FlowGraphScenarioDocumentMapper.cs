using FlowGraph.Core;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Maps JiM <see cref="IScenarioGraphDocument"/> + layout into FlowGraph.Core (adapter boundary from plan).</summary>
public static class FlowGraphScenarioDocumentMapper
{
	public static Graph ToFlowGraph( IScenarioGraphDocument document, ScenarioGraphLayoutResult layout )
	{
		var graph = new Graph();
		graph.BeginBatchLoad();

		foreach ( var n in document.Nodes )
		{
			if ( !layout.Positions.TryGetValue( n.Id, out var pos ) )
				pos = (0, 0);

			var fgNode = new Node
			{
				Id = n.Id,
				Type = FlowNodeType( n ),
				Label = n.Label,
				Position = new Point( pos.X, pos.Y ),
				Width = layout.DefaultNodeWidth,
				Height = layout.DefaultNodeHeight,
				IsResizable = false,
				IsConnectable = false,
				IsDeletable = false,
				Inputs = [new Port { Id = "in", Type = "data" }],
				Outputs = [new Port { Id = "out", Type = "data" }],
			};
			graph.AddNode( fgNode );
		}

		foreach ( var e in document.Edges )
		{
			var style = FlowEdgeStyle( e.Kind );
			graph.AddEdge( new Edge
			{
				Id = e.Id,
				Source = e.SourceNodeId,
				Target = e.TargetNodeId,
				SourcePort = "out",
				TargetPort = "in",
				Label = e.Label,
				Type = style.Type,
				MarkerEnd = style.MarkerEnd,
			} );
		}

		graph.EndBatchLoad();
		return graph;
	}

	static string FlowNodeType( ScenarioGraphNode node )
	{
		if ( !string.IsNullOrWhiteSpace( node.RenderType ) )
			return node.RenderType!;
		return node.Kind switch
		{
			ScenarioGraphNodeKind.Start => "input",
			ScenarioGraphNodeKind.ThreatLevel => "output",
			_ => "process",
		};
	}

	static (EdgeType Type, EdgeMarker MarkerEnd) FlowEdgeStyle( ScenarioGraphEdgeKind kind ) =>
		kind switch
		{
			ScenarioGraphEdgeKind.ThreatChain => (EdgeType.Straight, EdgeMarker.None),
			ScenarioGraphEdgeKind.Contains => (EdgeType.SmoothStep, EdgeMarker.None),
			ScenarioGraphEdgeKind.ExploresFromStart => (EdgeType.SmoothStep, EdgeMarker.Arrow),
			ScenarioGraphEdgeKind.Includes => (EdgeType.Step, EdgeMarker.None),
			_ => (EdgeType.Bezier, EdgeMarker.ArrowClosed),
		};
}
