using System.Collections.Generic;
using JiMEditor.Avalonia.ScenarioGraph;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class WpfTreeLayoutEngineTests
{
	[Fact]
	public void Star_from_start_spreads_children_horizontally_and_down()
	{
		var nodes = new List<ScenarioGraphNode>
		{
			new( ScenarioGraphDocumentBuilder.StartNodeId, ScenarioGraphNodeKind.Start, "START" ),
			new( "cA", ScenarioGraphNodeKind.Chapter, "Alpha" ),
			new( "cB", ScenarioGraphNodeKind.Chapter, "Bravo" ),
		};
		var edges = new List<ScenarioGraphEdge>
		{
			new( "e0", ScenarioGraphDocumentBuilder.StartNodeId, "cA", ScenarioGraphEdgeKind.ExploresFromStart, null ),
			new( "e1", ScenarioGraphDocumentBuilder.StartNodeId, "cB", ScenarioGraphEdgeKind.ExploresFromStart, null ),
		};
		var doc = new ScenarioGraphDocument( nodes, edges );
		var layout = new WpfTreeLayoutEngine { LevelGap = 50, SiblingGap = 12 }.Layout(
			doc,
			new ScenarioGraphLayoutOptions { OriginX = 10, OriginY = 20, DefaultNodeWidth = 120, DefaultNodeHeight = 60 } );

		Assert.NotEqual( layout.Positions["cA"].X, layout.Positions["cB"].X );
		Assert.True( layout.Positions["cA"].Y > layout.Positions[ScenarioGraphDocumentBuilder.StartNodeId].Y );
		Assert.True( layout.Positions["cB"].Y > layout.Positions[ScenarioGraphDocumentBuilder.StartNodeId].Y );
	}

	[Fact]
	public void Orphan_unreachable_from_start_is_placed_below_tree()
	{
		var nodes = new List<ScenarioGraphNode>
		{
			new( ScenarioGraphDocumentBuilder.StartNodeId, ScenarioGraphNodeKind.Start, "START" ),
			new( "c1", ScenarioGraphNodeKind.Chapter, "Only" ),
			new( "lonely", ScenarioGraphNodeKind.Trigger, "Floater" ),
		};
		var edges = new List<ScenarioGraphEdge>
		{
			new( "e0", ScenarioGraphDocumentBuilder.StartNodeId, "c1", ScenarioGraphEdgeKind.ExploresFromStart, null ),
		};
		var doc = new ScenarioGraphDocument( nodes, edges );
		var layout = new WpfTreeLayoutEngine { LevelGap = 40, OrphanTopMargin = 30 }.Layout( doc );

		var startY = layout.Positions[ScenarioGraphDocumentBuilder.StartNodeId].Y;
		var orphanY = layout.Positions["lonely"].Y;
		Assert.True( orphanY > startY + layout.DefaultNodeHeight + 20 );
	}
}
