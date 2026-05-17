using System.Collections.Generic;
using System.Linq;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>
/// Assigns layers by relaxing longest-path depth from <see cref="ScenarioGraphDocumentBuilder.StartNodeId"/> (session-only positions).
/// Cycles are tolerated by capping iterations and layer magnitude.
/// </summary>
public sealed class LayeredRelaxationLayoutEngine : IScenarioGraphLayoutEngine
{
	public int MaxRelaxIterations { get; init; } = 256;
	public int MaxLayer { get; init; } = 120;
	/// <summary>Horizontal gap between BFS “layer” bands (each band is <see cref="MaxRowsPerBand"/> nodes tall).</summary>
	public double BandStride { get; init; } = 188;

	public double RowHeight { get; init; } = 95;
	public double OriginX { get; init; } = 48;
	public double OriginY { get; init; } = 48;

	/// <summary>Max nodes stacked vertically before starting the next horizontal band within the same layer.</summary>
	public int MaxRowsPerBand { get; init; } = 12;

	/// <summary>Horizontal gap after each layer’s bands before the next layer starts.</summary>
	public double LayerGap { get; init; } = 40;

	public ScenarioGraphLayoutResult Layout( IScenarioGraphDocument document, ScenarioGraphLayoutOptions? options = null )
	{
		var o = options ?? new ScenarioGraphLayoutOptions();
		var nodeIds = document.Nodes.Select( n => n.Id ).ToHashSet();
		var layers = new Dictionary<string, int>();
		foreach ( var n in document.Nodes )
			layers[n.Id] = 0;

		if ( layers.ContainsKey( ScenarioGraphDocumentBuilder.StartNodeId ) )
			layers[ScenarioGraphDocumentBuilder.StartNodeId] = 0;

		var iterations = System.Math.Min( MaxRelaxIterations, System.Math.Max( nodeIds.Count * 2, 32 ) );
		for ( var it = 0; it < iterations; it++ )
		{
			var changed = false;
			foreach ( var e in document.Edges )
			{
				if ( !nodeIds.Contains( e.SourceNodeId ) || !nodeIds.Contains( e.TargetNodeId ) )
					continue;
				var s = layers[e.SourceNodeId];
				var candidate = System.Math.Min( s + 1, MaxLayer );
				if ( layers[e.TargetNodeId] < candidate )
				{
					layers[e.TargetNodeId] = candidate;
					changed = true;
				}
			}

			if ( !changed )
				break;
		}

		foreach ( var n in document.Nodes )
		{
			if ( n.Id == ScenarioGraphDocumentBuilder.StartNodeId )
				continue;
			var hasIncoming = false;
			foreach ( var e in document.Edges )
			{
				if ( e.TargetNodeId != n.Id )
					continue;
				hasIncoming = true;
				break;
			}

			if ( !hasIncoming && layers[n.Id] == 0 )
				layers[n.Id] = 1;
		}

		var byLayer = document.Nodes
			.GroupBy( n => layers.GetValueOrDefault( n.Id, 0 ) )
			.OrderBy( g => g.Key )
			.ToList();

		var positions = new Dictionary<string, (double X, double Y)>();
		var cursorX = OriginX;
		foreach ( var group in byLayer )
		{
			var ordered = group
				.OrderBy( n => n.Kind )
				.ThenBy( n => n.Label, System.StringComparer.OrdinalIgnoreCase )
				.ToList();
			var rows = System.Math.Max( 1, MaxRowsPerBand );
			var bandCount = (ordered.Count + rows - 1) / rows;
			for ( var i = 0; i < ordered.Count; i++ )
			{
				var band = i / rows;
				var row = i % rows;
				var x = cursorX + band * BandStride;
				var y = OriginY + row * RowHeight;
				positions[ordered[i].Id] = (x, y);
			}

			cursorX += bandCount * BandStride + LayerGap;
		}

		return new ScenarioGraphLayoutResult( positions, o.DefaultNodeWidth, o.DefaultNodeHeight );
	}
}
