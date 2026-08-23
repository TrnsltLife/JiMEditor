// JiMEditor Avalonia: summarize catalog references that would be invalidated when removing a Collection (not in WPF).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiME;

namespace JiMEditor.Avalonia.EventDialog;

/// <summary>
/// Counts of scenario references by category (merged across events). Keys are display names; values are occurrence counts.
/// </summary>
public sealed class CollectionRemovalImpact
{
	public Dictionary<string, int> Items { get; } = new( StringComparer.Ordinal );
	public Dictionary<string, int> Titles { get; } = new( StringComparer.Ordinal );
	public Dictionary<string, int> Tokens { get; } = new( StringComparer.Ordinal );
	public Dictionary<string, int> Tiles { get; } = new( StringComparer.Ordinal );
	public Dictionary<string, int> Enemies { get; } = new( StringComparer.Ordinal );

	public void Add( string category, string displayName, int count = 1 )
	{
		if ( string.IsNullOrWhiteSpace( displayName ) || count <= 0 )
			return;
		var d = GetBucket( category );
		if ( d == null )
			return;
		if ( d.TryGetValue( displayName, out var prev ) )
			d[displayName] = prev + count;
		else
			d[displayName] = count;
	}

	Dictionary<string, int>? GetBucket( string category ) =>
		category switch
		{
			"Items" => Items,
			"Titles" => Titles,
			"Tokens" => Tokens,
			"Tiles" => Tiles,
			"Enemies" => Enemies,
			_ => null
		};

	/// <summary>Merges parallel per-event impacts into one scenario-level result.</summary>
	public static CollectionRemovalImpact Merge( IEnumerable<CollectionRemovalImpact> parts )
	{
		var merged = new CollectionRemovalImpact();
		foreach ( var p in parts )
			merged.MergeFrom( p );
		return merged;
	}

	public void MergeFrom( CollectionRemovalImpact other )
	{
		MergeDict( Items, other.Items );
		MergeDict( Titles, other.Titles );
		MergeDict( Tokens, other.Tokens );
		MergeDict( Tiles, other.Tiles );
		MergeDict( Enemies, other.Enemies );
	}

	static void MergeDict( Dictionary<string, int> target, Dictionary<string, int> source )
	{
		foreach ( var kv in source )
		{
			if ( target.TryGetValue( kv.Key, out var t ) )
				target[kv.Key] = t + kv.Value;
			else
				target[kv.Key] = kv.Value;
		}
	}

	public bool IsEmpty =>
		Items.Count == 0 && Titles.Count == 0 && Tokens.Count == 0 && Tiles.Count == 0 && Enemies.Count == 0;

	/// <summary>Human-readable lines for a message box (Enemies → Items → Titles → Tokens → Tiles).</summary>
	public static string FormatForDialog( CollectionRemovalImpact impact )
	{
		var sb = new StringBuilder();
		AppendSection( sb, "Enemies", impact.Enemies );
		AppendSection( sb, "Items", impact.Items );
		AppendSection( sb, "Titles", impact.Titles );
		AppendSection( sb, "Tokens", impact.Tokens );
		AppendSection( sb, "Tiles", impact.Tiles );
		return sb.ToString().TrimEnd();
	}

	static void AppendSection( StringBuilder sb, string heading, Dictionary<string, int> dict )
	{
		if ( dict.Count == 0 )
			return;
		var parts = dict
			.OrderBy( kv => kv.Key, StringComparer.Ordinal )
			.Select( kv => kv.Value <= 1 ? kv.Key : $"{kv.Key} ({kv.Value})" );
		sb.Append( heading ).Append( ": " ).AppendJoin( ", ", parts ).AppendLine();
	}
}

/// <summary>
/// Parallel to <see cref="ScenarioCollectionAvailability"/> — estimates references tied to a collection being removed.
/// </summary>
public static class ScenarioCollectionRemovalImpact
{
	/// <summary>Aggregates impact across all top-level interactions in <paramref name="scenario"/>.</summary>
	public static CollectionRemovalImpact GetImpactForRemovingCollection( Scenario scenario, Collection removed )
	{
		var list = new List<CollectionRemovalImpact>();
		foreach ( var inter in scenario.interactionObserver )
			list.Add( GetImpactFromInteraction( scenario, inter, removed ) );
		return CollectionRemovalImpact.Merge( list );
	}

	/// <summary>Parallel to per-event pruning: impact from a single interaction (merge with <see cref="CollectionRemovalImpact.Merge"/> for custom walks).</summary>
	public static CollectionRemovalImpact GetImpactFromInteraction( Scenario scenario, IInteraction inter, Collection removed ) =>
		CollectFromInteraction( scenario, inter, removed );

	static CollectionRemovalImpact CollectFromInteraction( Scenario scenario, IInteraction inter, Collection removed )
	{
		var impact = new CollectionRemovalImpact();
		switch ( inter )
		{
			case ItemInteraction item:
				CollectItemInteraction( item, removed, impact );
				break;
			case TitleInteraction title:
				CollectTitleInteraction( title, removed, impact );
				break;
			case ThreatInteraction threat:
				CollectThreatInteraction( threat, removed, impact );
				break;
		}

		if ( inter is InteractionBase ib )
			CollectTokenInteraction( scenario, ib, removed, impact );

		return impact;
	}

	static void CollectItemInteraction( ItemInteraction item, Collection removed, CollectionRemovalImpact impact )
	{
		foreach ( var it in item.itemList )
		{
			var coll = Collection.FromID( it.collection );
			if ( coll == removed )
				impact.Add( "Items", it.dataName ?? "Item" );
		}
	}

	static void CollectTitleInteraction( TitleInteraction title, Collection removed, CollectionRemovalImpact impact )
	{
		foreach ( var t in title.titleList )
		{
			var coll = Collection.FromID( t.collection );
			if ( coll == removed )
				impact.Add( "Titles", t.dataName ?? "Title" );
		}
	}

	static void CollectThreatInteraction( ThreatInteraction threat, Collection removed, CollectionRemovalImpact impact )
	{
		var monsters = Collection.MONSTERS();
		for ( var i = 0; i < threat.includedEnemies.Length && i < monsters.Length; i++ )
		{
			if ( !threat.includedEnemies[i] )
				continue;
			var coll = CollectionForMonsterTypeId( monsters[i].id );
			if ( coll == removed )
				impact.Add( "Enemies", monsters[i].dataName ?? $"Enemy {i}" );
		}

		foreach ( var m in threat.monsterCollection )
		{
			var coll = CollectionForMonsterTypeId( m.id );
			if ( coll == removed )
				impact.Add( "Enemies", string.IsNullOrWhiteSpace( m.dataName ) ? $"Enemy type {m.id}" : m.dataName );
		}
	}

	static void CollectTokenInteraction( Scenario scenario, InteractionBase interaction, Collection removed, CollectionRemovalImpact impact )
	{
		if ( !interaction.isTokenInteraction )
			return;

		var afterRemoval = scenario.collectionObserver.Where( c => c != removed ).ToList();

		switch ( interaction.tokenType )
		{
			case TokenType.DifficultGround:
				if ( removed.DifficultGround && !afterRemoval.Any( c => c.DifficultGround ) )
					impact.Add( "Tokens", "Difficult Ground" );
				break;
			case TokenType.Fortified:
				if ( removed.Fortified && !afterRemoval.Any( c => c.Fortified ) )
					impact.Add( "Tokens", "Fortified" );
				break;
			case TokenType.Terrain:
			{
				var tc = Collection.FromTerrainType( interaction.terrainType );
				if ( tc == removed )
					impact.Add( "Tokens", TerrainTokenLabel( interaction.terrainType ) );
				break;
			}
		}
	}

	static string TerrainTokenLabel( TerrainType tt ) =>
		$"Terrain: {tt}";

		static Collection? CollectionForMonsterTypeId( int monsterTypeId ) =>
			Collection.FromMonsterTypeId( monsterTypeId );
}
