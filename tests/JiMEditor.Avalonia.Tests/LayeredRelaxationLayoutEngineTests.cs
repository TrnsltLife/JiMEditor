using System.Collections.Generic;
using JiMEditor.Avalonia.ScenarioGraph;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class LayeredRelaxationLayoutEngineTests
{
	[Fact]
	public void Dense_layer_splits_into_horizontal_bands()
	{
		var nodes = new List<ScenarioGraphNode>
		{
			new( ScenarioGraphDocumentBuilder.StartNodeId, ScenarioGraphNodeKind.Start, "START" ),
		};
		for ( var i = 0; i < 14; i++ )
			nodes.Add( new ScenarioGraphNode( $"c{i}", ScenarioGraphNodeKind.Chapter, $"C{i:D2}" ) );

		var edges = new List<ScenarioGraphEdge>();
		for ( var i = 0; i < 14; i++ )
		{
			edges.Add( new ScenarioGraphEdge(
				$"e{i}",
				ScenarioGraphDocumentBuilder.StartNodeId,
				$"c{i}",
				ScenarioGraphEdgeKind.ExploresFromStart,
				"explores" ) );
		}

		var doc = new ScenarioGraphDocument( nodes, edges );
		var layout = new LayeredRelaxationLayoutEngine
		{
			BandStride = 200,
			MaxRowsPerBand = 12,
			LayerGap = 50,
			OriginX = 10,
			OriginY = 5,
			RowHeight = 10,
		}.Layout( doc );

		Assert.True( layout.Positions["c12"].X > layout.Positions["c0"].X, "twelfth chapter should start second horizontal band" );
	}
}
