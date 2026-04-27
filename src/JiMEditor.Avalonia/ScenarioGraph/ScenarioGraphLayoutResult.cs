using System.Collections.Generic;
using FlowGraph.Core;

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

	/// <summary>
	/// When true, FlowGraph ports are anchored with outputs on the bottom and inputs on the top
	/// (parent above child), matching top-down tree layout. When false, FlowGraph defaults apply (input left, output right).
	/// </summary>
	public bool UseTopDownFlowPorts { get; init; }

	/// <summary>
	/// When true, the FlowGraph is built with nodes and ports only (no edges).
	/// Used to validate top/bottom ports and node interaction without edge routing.
	/// </summary>
	public bool OmitEdges { get; init; }

	/// <summary>
	/// Optional override applied to every edge's visual line type for the current render.
	/// Null uses per-edge defaults from <see cref="FlowGraphScenarioDocumentMapper"/>.
	/// </summary>
	public EdgeType? ForcedEdgeType { get; set; }
}
