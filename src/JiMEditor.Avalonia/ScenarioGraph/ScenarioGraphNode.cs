namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>One vertex in the editor scenario graph read model (no UI package types).</summary>
public sealed class ScenarioGraphNode
{
	public ScenarioGraphNode( string id, ScenarioGraphNodeKind kind, string label, string? renderType = null )
	{
		Id = id;
		Kind = kind;
		Label = label;
		RenderType = renderType;
	}

	public string Id { get; }
	public ScenarioGraphNodeKind Kind { get; }
	public string Label { get; }
	public string? RenderType { get; }
}
