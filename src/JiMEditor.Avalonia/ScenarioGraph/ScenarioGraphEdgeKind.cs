namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Semantic edge categories distilled from WPF graph edge labels (see docs/ScenarioGraphSpec.md).</summary>
public enum ScenarioGraphEdgeKind
{
	/// <summary>Unclassified or future extension.</summary>
	Other,

	/// <summary>Scenario START → tile block when the chapter has no <c>triggeredBy</c> (WPF: "explores").</summary>
	ExploresFromStart,

	// Common WPF labels mapped for future full-graph builds:
	Initiates,
	Completes,
	OnCompletion,
	Contains,
	Reveals,
	Triggers,
	Explores,
	ExploreTriggers,
	FullyExploredTriggers,
	Activates,
	Input,
	Condition,
	ThreatChain,

	/// <summary>Interaction group → interaction (WPF: "includes").</summary>
	Includes,

	/// <summary>BranchInteraction branch output (WPF: "set").</summary>
	Set,

	/// <summary>BranchInteraction branch output (WPF: "not set").</summary>
	NotSet,

	/// <summary>ConditionalInteraction → trigger (WPF: "finished").</summary>
	Finished,
}
