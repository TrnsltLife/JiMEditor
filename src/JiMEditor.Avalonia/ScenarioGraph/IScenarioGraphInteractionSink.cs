using System.Threading.Tasks;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Receives graph-node activation events and routes them into Scenario-backed editor actions.</summary>
public interface IScenarioGraphInteractionSink
{
	Task OnNodeActivatedAsync( ScenarioGraphNode node );
}
