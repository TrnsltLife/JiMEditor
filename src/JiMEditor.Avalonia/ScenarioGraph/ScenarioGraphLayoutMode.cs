namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Session-only layout strategy for the scenario graph (not persisted).</summary>
public enum ScenarioGraphLayoutMode
{
	/// <summary>BFS-style layers with horizontal banding (current default).</summary>
	LayeredRelaxation,

	/// <summary>Rooted tree from START using a BFS spanning tree; closer to WPF tree spreading.</summary>
	WpfTree,

	/// <summary>Same positions as <see cref="WpfTree"/> but nodes-only: top/bottom ports, no edges (experiment).</summary>
	Tree2,
}
