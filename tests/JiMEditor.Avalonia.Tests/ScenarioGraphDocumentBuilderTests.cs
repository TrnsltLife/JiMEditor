using JiMEditor.Avalonia.ScenarioGraph;
using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class ScenarioGraphDocumentBuilderTests
{
	[Fact]
	public void TileBlocksAndStart_empty_scenario_has_only_start_node()
	{
		var s = new Scenario( "Graph test" );
		var doc = ScenarioGraphDocumentBuilder.Build( s, ScenarioGraphBuildScope.TileBlocksAndStart );

		Assert.Single( doc.Nodes );
		Assert.Equal( ScenarioGraphDocumentBuilder.StartNodeId, doc.Nodes[0].Id );
		Assert.Equal( ScenarioGraphNodeKind.Start, doc.Nodes[0].Kind );
		Assert.Empty( doc.Edges );
	}

	[Fact]
	public void TileBlocksAndStart_adds_explores_edge_when_triggeredBy_none_or_empty()
	{
		var s = new Scenario( "Graph test" );
		var chapter = new Chapter( "Block A" )
		{
			GUID = Guid.Parse( "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA" ),
		};
		s.chapterObserver.Add( chapter );

		var doc = ScenarioGraphDocumentBuilder.Build( s, ScenarioGraphBuildScope.TileBlocksAndStart );

		Assert.Equal( 2, doc.Nodes.Count );
		Assert.Equal( ScenarioGraphNodeKind.Chapter, doc.Nodes[1].Kind );
		Assert.Single( doc.Edges );
		Assert.Equal( ScenarioGraphEdgeKind.ExploresFromStart, doc.Edges[0].Kind );
		Assert.Equal( ScenarioGraphDocumentBuilder.StartNodeId, doc.Edges[0].SourceNodeId );
		Assert.Equal( ScenarioGraphFullScenarioDocumentBuilder.ChapterId( chapter.dataName ), doc.Edges[0].TargetNodeId );
	}

	[Fact]
	public void TileBlocksAndStart_no_edge_when_triggeredBy_set()
	{
		var s = new Scenario( "Graph test" );
		s.chapterObserver.Add( new Chapter( "Later" ) { triggeredBy = "SomeTrigger" } );

		var doc = ScenarioGraphDocumentBuilder.Build( s, ScenarioGraphBuildScope.TileBlocksAndStart );

		Assert.Equal( 2, doc.Nodes.Count );
		Assert.Empty( doc.Edges );
	}

	[Fact]
	public void HorizontalStripLayout_assigns_positions_in_node_order()
	{
		var nodes = new[]
		{
			new ScenarioGraphNode( "a", ScenarioGraphNodeKind.Start, "START" ),
			new ScenarioGraphNode( "b", ScenarioGraphNodeKind.Chapter, "C1" ),
		};
		var doc = new ScenarioGraphDocument( nodes, [] );
		var layout = new HorizontalStripLayoutEngine().Layout( doc, new ScenarioGraphLayoutOptions { OriginX = 10, StepX = 100 } );

		Assert.Equal( 10, layout.Positions["a"].X );
		Assert.Equal( 110, layout.Positions["b"].X );
	}

	[Fact]
	public void FullScenario_includes_triggers_objectives_and_default_start_chapter()
	{
		var s = new Scenario( "Full graph test" );
		s.chapterObserver.Add( new Chapter( "Start" ) );
		s.triggersObserver.Add( new Trigger( "T1" ) );
		s.objectiveObserver.Add( new Objective( "Obj1" ) { triggeredByName = "T1" } );

		var doc = ScenarioGraphDocumentBuilder.Build( s, ScenarioGraphBuildScope.FullScenario );

		Assert.Contains( doc.Nodes, n => n.Id == ScenarioGraphDocumentBuilder.StartNodeId );
		Assert.Contains( doc.Nodes, n => n.Id == ScenarioGraphFullScenarioDocumentBuilder.TriggerId( "T1" ) );
		Assert.Contains( doc.Nodes, n => n.Id == ScenarioGraphFullScenarioDocumentBuilder.ObjectiveId( "Obj1" ) );
		Assert.Contains( doc.Nodes, n => n.Id == ScenarioGraphFullScenarioDocumentBuilder.ChapterId( "Start" ) );
		Assert.Contains( doc.Edges, e => e.SourceNodeId == ScenarioGraphFullScenarioDocumentBuilder.TriggerId( "T1" )
		                                  && e.TargetNodeId == ScenarioGraphFullScenarioDocumentBuilder.ObjectiveId( "Obj1" ) );
	}
}
