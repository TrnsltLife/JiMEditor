namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Which slice of the scenario participates in the graph document (expand over time).</summary>
public enum ScenarioGraphBuildScope
{
	/// <summary>START + tile blocks (chapters) and chapter-only edges (smoke tests / minimal UI).</summary>
	TileBlocksAndStart,

	/// <summary>Full parity with WPF <c>JiME.Visualization.Graph.Generate</c> (triggers, objectives, resolutions, interactions, chapters/tiles/tokens, threat ladder).</summary>
	FullScenario,
}
