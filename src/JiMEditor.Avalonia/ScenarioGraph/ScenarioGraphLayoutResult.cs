using System.Collections.Generic;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Ephemeral layout coordinates (session-only; never persisted in .jime).</summary>
public sealed class ScenarioGraphLayoutResult
{
	public ScenarioGraphLayoutResult(
		IReadOnlyDictionary<string, (double X, double Y)> positions,
		double defaultNodeWidth,
		double defaultNodeHeight )
	{
		Positions = positions;
		DefaultNodeWidth = defaultNodeWidth;
		DefaultNodeHeight = defaultNodeHeight;
	}

	public IReadOnlyDictionary<string, (double X, double Y)> Positions { get; }
	public double DefaultNodeWidth { get; }
	public double DefaultNodeHeight { get; }
}
