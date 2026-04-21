namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Swappable graph UI host (FlowGraph today; another library could implement the same contract).</summary>
public interface IScenarioGraphViewHost
{
	void Present( IScenarioGraphDocument document, ScenarioGraphLayoutResult layout );
	void SetInteractionSink( IScenarioGraphInteractionSink? sink );
}
