using System.Collections.Generic;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Places nodes in one row following <see cref="IScenarioGraphDocument.Nodes"/> order.</summary>
public sealed class HorizontalStripLayoutEngine : IScenarioGraphLayoutEngine
{
	public ScenarioGraphLayoutResult Layout( IScenarioGraphDocument document, ScenarioGraphLayoutOptions? options = null )
	{
		var o = options ?? new ScenarioGraphLayoutOptions();
		var positions = new Dictionary<string, (double X, double Y)>();
		var x = o.OriginX;

		foreach ( var node in document.Nodes )
		{
			positions[node.Id] = (x, o.OriginY);
			x += o.StepX;
		}

		return new ScenarioGraphLayoutResult( positions, o.DefaultNodeWidth, o.DefaultNodeHeight );
	}
}
