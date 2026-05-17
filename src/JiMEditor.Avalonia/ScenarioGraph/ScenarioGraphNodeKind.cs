namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>JiM-owned vertex classification (parity with WPF DataVertex.Type in JiME.Visualization).</summary>
public enum ScenarioGraphNodeKind
{
	Start,
	Trigger,
	Objective,
	Resolution,
	Interaction,
	InteractionGroup,
	Chapter,
	Tile,
	Token,
	ThreatLevel,
}
