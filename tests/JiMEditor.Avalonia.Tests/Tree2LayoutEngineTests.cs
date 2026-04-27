using System.Collections.Generic;
using JiMEditor.Avalonia.ScenarioGraph;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class Tree2LayoutEngineTests
{
	[Fact]
	public void Tree2_positions_match_wpf_tree_defaults_and_sets_flags()
	{
		var nodes = new List<ScenarioGraphNode>
		{
			new( ScenarioGraphDocumentBuilder.StartNodeId, ScenarioGraphNodeKind.Start, "START" ),
			new( "cA", ScenarioGraphNodeKind.Chapter, "Alpha" ),
		};
		var edges = new List<ScenarioGraphEdge>
		{
			new( "e0", ScenarioGraphDocumentBuilder.StartNodeId, "cA", ScenarioGraphEdgeKind.ExploresFromStart, null ),
		};
		var doc = new ScenarioGraphDocument( nodes, edges );

		var tree = new WpfTreeLayoutEngine().Layout( doc );
		var tree2 = new Tree2LayoutEngine().Layout( doc );

		Assert.True( tree.UseTopDownFlowPorts );
		Assert.True( tree2.UseTopDownFlowPorts );
		Assert.True( tree2.OmitEdges );
		Assert.False( tree.OmitEdges );

		Assert.Equal( tree.Positions.Count, tree2.Positions.Count );
		foreach ( var kv in tree.Positions )
			Assert.Equal( kv.Value, tree2.Positions[kv.Key] );
	}
}
