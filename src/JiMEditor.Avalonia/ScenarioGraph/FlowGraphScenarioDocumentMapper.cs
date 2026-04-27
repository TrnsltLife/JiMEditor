using FlowGraph.Core;
using FlowGraph.Core.Models;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Maps JiM <see cref="IScenarioGraphDocument"/> + layout into FlowGraph.Core (adapter boundary from plan).</summary>
public static class FlowGraphScenarioDocumentMapper
{
	// Match FlowGraph examples ("in" / "out"); port screen position comes from <see cref="PortDefinition.Position"/>.
	const string InputPortId = "in";
	const string OutputPortId = "out";

	public static Graph ToFlowGraph( IScenarioGraphDocument document, ScenarioGraphLayoutResult layout )
	{
		var graph = new Graph();
		graph.BeginBatchLoad();

		foreach ( var n in document.Nodes )
		{
			if ( !layout.Positions.TryGetValue( n.Id, out var pos ) )
				pos = (0, 0);

			var topDownPorts = layout.UseTopDownFlowPorts;
			var fgNode = new Node(
				new NodeDefinition
				{
					Id = n.Id,
					Type = FlowNodeType( n ),
					Label = n.Label,
					IsResizable = false,
					IsDraggable = true,
					IsSelectable = true,
					IsConnectable = false,
					IsDeletable = false,
					Inputs =
					[
						new PortDefinition
						{
							Id = InputPortId,
							Type = "data",
							Position = topDownPorts ? PortPosition.Top : null,
						},
					],
					Outputs =
					[
						new PortDefinition
						{
							Id = OutputPortId,
							Type = "data",
							Position = topDownPorts ? PortPosition.Bottom : null,
						},
					],
				} )
			{
				Position = new Point( pos.X, pos.Y ),
				Width = layout.DefaultNodeWidth,
				Height = layout.DefaultNodeHeight,
			};
			graph.AddNode( fgNode );
		}

		if ( !layout.OmitEdges )
		{
			var topDown = layout.UseTopDownFlowPorts;
			foreach ( var e in document.Edges )
			{
				var style = FlowEdgeStyle( e.Kind, topDown );
				var edge = new Edge(
					new EdgeDefinition
					{
						Id = e.Id,
						Source = e.SourceNodeId,
						Target = e.TargetNodeId,
						SourcePort = OutputPortId,
						TargetPort = InputPortId,
						Label = e.Label,
						Type = layout.ForcedEdgeType ?? style.Type,
						MarkerEnd = style.MarkerEnd,
					} )
				{
					IsSelectable = false,
				};
				graph.AddEdge( edge );
			}
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

	static (EdgeType Type, EdgeMarker MarkerEnd) FlowEdgeStyle( ScenarioGraphEdgeKind kind, bool topDownPorts ) =>
		kind switch
		{
			ScenarioGraphEdgeKind.ThreatChain => (EdgeType.Straight, EdgeMarker.None),
			ScenarioGraphEdgeKind.Contains => (EdgeType.SmoothStep, EdgeMarker.None),
			ScenarioGraphEdgeKind.ExploresFromStart => (EdgeType.SmoothStep, EdgeMarker.Arrow),
			ScenarioGraphEdgeKind.Includes => topDownPorts
				? (EdgeType.SmoothStep, EdgeMarker.None)
				: (EdgeType.Step, EdgeMarker.None),
			_ => topDownPorts
				? (EdgeType.SmoothStep, EdgeMarker.ArrowClosed)
				: (EdgeType.Bezier, EdgeMarker.ArrowClosed),
		};
}
