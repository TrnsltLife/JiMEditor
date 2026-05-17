using System;
using System.Collections.Generic;
using System.Linq;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>
/// Top-down tree layout rooted at <see cref="ScenarioGraphDocumentBuilder.StartNodeId"/> using a BFS spanning tree
/// (first discovery wins on directed edges). Nodes unreachable from START are laid out in a strip below the tree.
/// Positions are session-only.
/// </summary>
public sealed class WpfTreeLayoutEngine : IScenarioGraphLayoutEngine
{
	/// <summary>Vertical distance between parent row and child row (larger makes edge routing readable).</summary>
	public double LevelGap { get; init; } = 168;
	public double SiblingGap { get; init; } = 20;
	public double OrphanTopMargin { get; init; } = 88;
	public double OrphanRowGap { get; init; } = 44;
	public double OrphanWrapWidth { get; init; } = 2200;

	public ScenarioGraphLayoutResult Layout( IScenarioGraphDocument document, ScenarioGraphLayoutOptions? options = null )
	{
		var o = options ?? new ScenarioGraphLayoutOptions();
		var nodeW = o.DefaultNodeWidth;
		var nodeH = o.DefaultNodeHeight;
		var minSlot = nodeW + 24;

		if ( document.Nodes.Count == 0 )
			return new ScenarioGraphLayoutResult(
				new Dictionary<string, (double X, double Y)>( StringComparer.Ordinal ),
				o.DefaultNodeWidth,
				o.DefaultNodeHeight );

		var nodesById = document.Nodes.ToDictionary( n => n.Id, StringComparer.Ordinal );
		var treeChildren = document.Nodes.ToDictionary( n => n.Id, _ => new List<string>(), StringComparer.Ordinal );

		var outgoing = new Dictionary<string, List<string>>( StringComparer.Ordinal );
		foreach ( var n in document.Nodes )
			outgoing[n.Id] = new List<string>();

		foreach ( var e in document.Edges )
		{
			if ( !nodesById.ContainsKey( e.SourceNodeId ) || !nodesById.ContainsKey( e.TargetNodeId ) ) continue;
			outgoing[e.SourceNodeId].Add( e.TargetNodeId );
		}

		foreach ( var list in outgoing.Values )
		{
			list.Sort( ( a, b ) =>
			{
				var na = nodesById[a];
				var nb = nodesById[b];
				var k = na.Kind.CompareTo( nb.Kind );
				if ( k != 0 ) return k;
				var l = string.Compare( na.Label, nb.Label, StringComparison.OrdinalIgnoreCase );
				if ( l != 0 ) return l;
				return string.CompareOrdinal( na.Id, nb.Id );
			} );
		}

		var visited = new HashSet<string>( StringComparer.Ordinal );
		var rootId = ScenarioGraphDocumentBuilder.StartNodeId;
		if ( !nodesById.ContainsKey( rootId ) )
			rootId = document.Nodes.OrderBy( n => n.Kind ).ThenBy( n => n.Label, StringComparer.OrdinalIgnoreCase ).First().Id;

		var q = new Queue<string>();
		if ( nodesById.ContainsKey( rootId ) )
		{
			visited.Add( rootId );
			q.Enqueue( rootId );
		}

		while ( q.Count > 0 )
		{
			var u = q.Dequeue();
			foreach ( var v in outgoing[u] )
			{
				if ( visited.Contains( v ) ) continue;
				visited.Add( v );
				treeChildren[u].Add( v );
				q.Enqueue( v );
			}
		}

		var subtreeW = new Dictionary<string, double>( StringComparer.Ordinal );

		double Measure( string id )
		{
			if ( subtreeW.TryGetValue( id, out var memo ) ) return memo;
			var ch = treeChildren[id];
			if ( ch.Count == 0 )
			{
				memo = minSlot;
				subtreeW[id] = memo;
				return memo;
			}

			var span = 0.0;
			for ( var i = 0; i < ch.Count; i++ )
			{
				if ( i > 0 ) span += SiblingGap;
				span += Measure( ch[i] );
			}

			memo = Math.Max( minSlot, span );
			subtreeW[id] = memo;
			return memo;
		}

		if ( visited.Count > 0 )
			Measure( rootId );

		var positions = new Dictionary<string, (double X, double Y)>( StringComparer.Ordinal );

		double Assign( string id, double leftX, double y )
		{
			var totalW = subtreeW[id];
			var ch = treeChildren[id];
			if ( ch.Count == 0 )
			{
				positions[id] = ( leftX + ( totalW - nodeW ) / 2, y );
				return leftX + totalW;
			}

			var childSpan = 0.0;
			for ( var i = 0; i < ch.Count; i++ )
			{
				if ( i > 0 ) childSpan += SiblingGap;
				childSpan += subtreeW[ch[i]];
			}

			var childStart = leftX + ( totalW - childSpan ) / 2;
			var x = childStart;
			foreach ( var c in ch )
			{
				x = Assign( c, x, y + LevelGap );
				x += SiblingGap;
			}

			positions[id] = ( leftX + ( totalW - nodeW ) / 2, y );
			return leftX + totalW;
		}

		if ( visited.Contains( rootId ) )
			Assign( rootId, o.OriginX, o.OriginY );

		var orphans = document.Nodes
			.Select( n => n.Id )
			.Where( id => !visited.Contains( id ) )
			.OrderBy( id => nodesById[id].Kind )
			.ThenBy( id => nodesById[id].Label, StringComparer.OrdinalIgnoreCase )
			.ThenBy( id => id, StringComparer.Ordinal )
			.ToList();

		double treeBottom = o.OriginY;
		if ( positions.Count > 0 )
			treeBottom = positions.Values.Max( p => p.Y + nodeH );

		var ox = o.OriginX;
		var oy = treeBottom + OrphanTopMargin;
		var rowBottom = oy + nodeH;
		foreach ( var oid in orphans )
		{
			if ( ox > o.OriginX && ox + minSlot > o.OriginX + OrphanWrapWidth )
			{
				ox = o.OriginX;
				oy = rowBottom + OrphanRowGap;
				rowBottom = oy + nodeH;
			}

			if ( !subtreeW.ContainsKey( oid ) )
				subtreeW[oid] = minSlot;
			Assign( oid, ox, oy );
			ox += subtreeW[oid] + SiblingGap;
			rowBottom = Math.Max( rowBottom, oy + nodeH );
		}

		foreach ( var n in document.Nodes )
		{
			if ( !positions.ContainsKey( n.Id ) )
				positions[n.Id] = ( o.OriginX, o.OriginY );
		}

		return new ScenarioGraphLayoutResult( positions, o.DefaultNodeWidth, o.DefaultNodeHeight )
		{
			UseTopDownFlowPorts = true,
		};
	}
}
