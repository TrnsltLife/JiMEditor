namespace JiMEditor.Avalonia.ScenarioGraph;

public interface IScenarioGraphLayoutEngine
{
	ScenarioGraphLayoutResult Layout( IScenarioGraphDocument document, ScenarioGraphLayoutOptions? options = null );
}
