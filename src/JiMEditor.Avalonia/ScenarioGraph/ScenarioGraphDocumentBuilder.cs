using System;
using System.Collections.Generic;
using JiME;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Builds <see cref="IScenarioGraphDocument"/> from a <see cref="Scenario"/> (no UI state, no .jime fields).</summary>
public static class ScenarioGraphDocumentBuilder
{
	public const string StartNodeId = "jime:graph:start";

	public static IScenarioGraphDocument Build( Scenario scenario, ScenarioGraphBuildScope scope )
	{
		return scope switch
		{
			ScenarioGraphBuildScope.TileBlocksAndStart => BuildTileBlocksAndStart( scenario ),
			ScenarioGraphBuildScope.FullScenario => ScenarioGraphFullScenarioDocumentBuilder.Build( scenario ),
			_ => throw new ArgumentOutOfRangeException( nameof( scope ), scope, null ),
		};
	}

	static IScenarioGraphDocument BuildTileBlocksAndStart( Scenario scenario )
	{
		var nodes = new List<ScenarioGraphNode>
		{
			new( StartNodeId, ScenarioGraphNodeKind.Start, "START" ),
		};
		var edges = new List<ScenarioGraphEdge>();

		foreach ( var chapter in scenario.chapterObserver )
		{
			var id = ScenarioGraphFullScenarioDocumentBuilder.ChapterId( chapter.dataName );
			var label = string.IsNullOrWhiteSpace( chapter.dataName ) ? "(Tile block)" : chapter.dataName.Trim();
			nodes.Add( new ScenarioGraphNode( id, ScenarioGraphNodeKind.Chapter, label ) );

			if ( IsAutoExploredFromStart( chapter.triggeredBy ) )
			{
				edges.Add( new ScenarioGraphEdge(
					id: $"edge:start-explores:{chapter.GUID:N}",
					sourceNodeId: StartNodeId,
					targetNodeId: id,
					kind: ScenarioGraphEdgeKind.ExploresFromStart,
					label: "explores" ) );
			}
		}

		return new ScenarioGraphDocument( nodes, edges );
	}

	static bool IsAutoExploredFromStart( string? triggeredBy ) =>
		string.IsNullOrEmpty( triggeredBy ) || triggeredBy == "None";
}
