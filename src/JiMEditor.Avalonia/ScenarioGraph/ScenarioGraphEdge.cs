namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>One directed edge in the editor scenario graph read model.</summary>
public sealed class ScenarioGraphEdge
{
	public ScenarioGraphEdge( string id, string sourceNodeId, string targetNodeId, ScenarioGraphEdgeKind kind, string? label = null )
	{
		Id = id;
		SourceNodeId = sourceNodeId;
		TargetNodeId = targetNodeId;
		Kind = kind;
		Label = label;
	}

	public string Id { get; }
	public string SourceNodeId { get; }
	public string TargetNodeId { get; }
	public ScenarioGraphEdgeKind Kind { get; }
	public string? Label { get; }
}
