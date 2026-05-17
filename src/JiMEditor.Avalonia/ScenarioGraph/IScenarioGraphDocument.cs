using System.Collections.Generic;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Read-only scenario graph document: pure data, no FlowGraph / GraphX types.</summary>
public interface IScenarioGraphDocument
{
	IReadOnlyList<ScenarioGraphNode> Nodes { get; }
	IReadOnlyList<ScenarioGraphEdge> Edges { get; }
}
