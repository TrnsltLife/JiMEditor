namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>
/// Reuses <see cref="WpfTreeLayoutEngine"/> coordinates but marks the result for a nodes-only FlowGraph
/// (top/bottom ports, no edges) so routing and edge visuals do not run.
/// </summary>
public sealed class Tree2LayoutEngine : IScenarioGraphLayoutEngine
{
	readonly WpfTreeLayoutEngine _tree = new();

	public ScenarioGraphLayoutResult Layout( IScenarioGraphDocument document, ScenarioGraphLayoutOptions? options = null )
	{
		var inner = _tree.Layout( document, options );
		return new ScenarioGraphLayoutResult( inner.Positions, inner.DefaultNodeWidth, inner.DefaultNodeHeight )
		{
			UseTopDownFlowPorts = true,
			OmitEdges = true,
		};
	}
}
