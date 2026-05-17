using System.Collections.Generic;

namespace JiMEditor.Avalonia.ScenarioGraph;

public sealed class ScenarioGraphDocument : IScenarioGraphDocument
{
	public ScenarioGraphDocument( IReadOnlyList<ScenarioGraphNode> nodes, IReadOnlyList<ScenarioGraphEdge> edges )
	{
		Nodes = nodes;
		Edges = edges;
	}

	public IReadOnlyList<ScenarioGraphNode> Nodes { get; }
	public IReadOnlyList<ScenarioGraphEdge> Edges { get; }
}
