using System;
using System.Collections.Generic;
using System.Linq;
using JiME;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>
/// Full scenario graph (WPF parity with <c>JiME.Visualization.Graph.Generate</c>), JiM-owned read model only.
/// Ported from: JiMEditor/JiME/Models/Visualization/Graph.cs <c>Generate</c> (2026-04-20).
/// </summary>
public static class ScenarioGraphFullScenarioDocumentBuilder
{
	public static IScenarioGraphDocument Build( Scenario scenario )
	{
		var nodes = new List<ScenarioGraphNode>();
		var edges = new List<ScenarioGraphEdge>();
		var ids = new HashSet<string>( StringComparer.Ordinal );
		long edgeSeq = 0;
		string NextEdgeId() => $"jime:graph:edge:{edgeSeq++}";

		void AddNode( string id, ScenarioGraphNodeKind kind, string label, string? renderType = null )
		{
			if ( !ids.Add( id ) )
				return;
			nodes.Add( new ScenarioGraphNode( id, kind, label, renderType ) );
		}

		void AddEdge( string sourceId, string targetId, ScenarioGraphEdgeKind kind, string? label )
		{
			if ( !ids.Contains( sourceId ) || !ids.Contains( targetId ) )
				return;
			edges.Add( new ScenarioGraphEdge( NextEdgeId(), sourceId, targetId, kind, label ) );
		}

		void TryTrigger( string? dataName, Action<string> onExistingId )
		{
			if ( string.IsNullOrEmpty( dataName ) || dataName == "None" )
				return;
			var key = TriggerId( dataName );
			if ( ids.Contains( key ) )
				onExistingId( key );
		}

		void TryEvent( string? dataName, Action<string> onExistingId )
		{
			if ( string.IsNullOrEmpty( dataName ) || dataName == "None" )
				return;
			var key = InteractionId( dataName );
			if ( ids.Contains( key ) )
				onExistingId( key );
		}

		AddNode( ScenarioGraphDocumentBuilder.StartNodeId, ScenarioGraphNodeKind.Start, "SCENARIO" );

		foreach ( var t in scenario.triggersObserver )
		{
			if ( t.dataName == "None" )
				continue;
			// Use raw trigger name only (no extra suffix/parenthetical decorations).
			AddNode( TriggerId( t.dataName ), ScenarioGraphNodeKind.Trigger, WithHeader( "Trigger", t.dataName ), "trigger-arrow" );
		}

		foreach ( var o in scenario.objectiveObserver )
		{
			if ( o.dataName == "None" )
				continue;
			AddNode( ObjectiveId( o.dataName ), ScenarioGraphNodeKind.Objective, WithHeader( "Objective", o.dataName ) );
		}

		foreach ( var o in scenario.objectiveObserver )
		{
			if ( o.dataName == "None" )
				continue;
			var vId = ObjectiveId( o.dataName );
			if ( scenario.objectiveName == o.dataName )
				AddEdge( ScenarioGraphDocumentBuilder.StartNodeId, vId, ScenarioGraphEdgeKind.Initiates, "initiates" );

			TryTrigger( o.triggeredByName, trig => AddEdge( trig, vId, ScenarioGraphEdgeKind.Initiates, "initiates" ) );
			TryTrigger( o.triggerName, trig => AddEdge( trig, vId, ScenarioGraphEdgeKind.Completes, "completes" ) );
			TryTrigger( o.nextTrigger, trig => AddEdge( vId, trig, ScenarioGraphEdgeKind.OnCompletion, "on completion" ) );
		}

		foreach ( var r in scenario.resolutionObserver )
		{
			AddNode( ResolutionId( r.dataName ), ScenarioGraphNodeKind.Resolution, WithHeader( "Resolution", r.dataName ), "resolution-rect" );
		}

		foreach ( var r in scenario.resolutionObserver )
		{
			var vId = ResolutionId( r.dataName );
			TryTrigger( r.triggerName, trig => AddEdge( trig, vId, ScenarioGraphEdgeKind.Triggers, "triggers" ) );
		}

		foreach ( var x in scenario.interactionObserver )
		{
			if ( x.dataName == "None" )
				continue;
			var vId = InteractionId( x.dataName );
			AddNode( vId, ScenarioGraphNodeKind.Interaction, WithHeader( "Event", x.dataName ), "event-rect" );
			var groupId = EnsureInteractionGroupFromInteractionName( x.dataName, nodes, ids );
			if ( groupId != null )
				AddEdge( groupId, vId, ScenarioGraphEdgeKind.Includes, "includes" );
		}

		foreach ( var x in scenario.interactionObserver )
		{
			if ( x.dataName == "None" )
				continue;
			if ( x is not InteractionBase x2 )
				continue;
			var vId = InteractionId( x.dataName );
			TryTrigger( x2.triggerName, trig => AddEdge( trig, vId, ScenarioGraphEdgeKind.Initiates, "initiates" ) );
			TryTrigger( x2.triggerAfterName, trig => AddEdge( vId, trig, ScenarioGraphEdgeKind.Triggers, "triggers" ) );

			switch ( x )
			{
				case DecisionInteraction d:
					TryTrigger( d.choice1Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice1 + "\"" ) );
					TryTrigger( d.choice2Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice2 + "\"" ) );
					TryTrigger( d.choice3Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice3 + "\"" ) );
					break;
				case DialogInteraction d:
					TryTrigger( d.c1Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice1 + "\"" ) );
					TryTrigger( d.c2Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice2 + "\"" ) );
					TryTrigger( d.c3Trigger, t => AddEdge( vId, t, ScenarioGraphEdgeKind.Other, "\"" + d.choice3 + "\"" ) );
					break;
				case TestInteraction t:
					TryTrigger( t.successTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"PASS\"" ) );
					TryTrigger( t.failTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"FAIL\"" ) );
					break;
				case ConditionalInteraction c:
					TryTrigger( c.finishedTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Finished, "finished" ) );
					if ( c.triggerList != null )
					{
						foreach ( var tName in c.triggerList )
							TryTrigger( tName, tr => AddEdge( tr, vId, ScenarioGraphEdgeKind.Condition, "condition" ) );
					}

					break;
				case PersistentTokenInteraction p:
					TryEvent( p.eventToActivate, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Other, "\"activate\"" ) );
					TryTrigger( p.alternativeTextTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"alt.text\"" ) );
					break;
				case ThreatInteraction th:
					TryTrigger( th.triggerDefeatedName, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"defeated\"" ) );
					break;
				case MultiEventInteraction m:
					if ( m.triggerList != null && m.usingTriggers )
					{
						foreach ( var tName in m.triggerList )
							TryTrigger( tName, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Triggers, "triggers" ) );
					}

					if ( m.eventList != null && !m.usingTriggers )
					{
						foreach ( var evName in m.eventList )
							TryEvent( evName, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Triggers, "triggers" ) );
					}

					break;
				case BranchInteraction b:
					TryTrigger( b.triggerTest, tr => AddEdge( tr, vId, ScenarioGraphEdgeKind.Input, "input" ) );
					if ( b.branchTestEvent )
					{
						TryEvent( b.triggerIsSet, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Set, "set" ) );
						TryEvent( b.triggerNotSet, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.NotSet, "not set" ) );
					}
					else
					{
						TryTrigger( b.triggerIsSetTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Set, "set" ) );
						TryTrigger( b.triggerNotSetTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.NotSet, "not set" ) );
					}

					break;
				case ReplaceTokenInteraction rp:
					TryEvent( rp.eventToReplace, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Other, "replace this" ) );
					TryEvent( rp.replaceWithEvent, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Other, "with this" ) );
					break;
				case ItemInteraction it:
					TryTrigger( it.fallbackTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"fallback\"" ) );
					break;
				case TitleInteraction ti:
					TryTrigger( ti.fallbackTrigger, tr => AddEdge( vId, tr, ScenarioGraphEdgeKind.Other, "\"fallback\"" ) );
					break;
			}
		}

		foreach ( var ch in scenario.chapterObserver )
		{
			var chId = ChapterId( ch.dataName );
			var chapterName = string.IsNullOrWhiteSpace( ch.dataName ) ? "(unnamed)" : ch.dataName.Trim();
			var chapterLabel = WithHeader( "Tile Block", chapterName );
			var chapterRenderType = scenario.scenarioTypeJourney ? "chapter-hex" : "chapter-square";
			AddNode( chId, ScenarioGraphNodeKind.Chapter, chapterLabel, chapterRenderType );
		}

		foreach ( var ch in scenario.chapterObserver )
		{
			var chId = ChapterId( ch.dataName );
			foreach ( var tile in ch.tileObserver.OfType<BaseTile>() )
			{
				var tileId = TileId( tile.GUID );
				var tileLabel = WithHeader( "Tile", tile.idNumber.ToString() + tile.tileSide );
				var tileRenderType = ( tile.tileType == TileType.Battle || tile.idNumber == 998 || tile.idNumber == 999 )
					? "tile-square"
					: "tile-hex";
				AddNode( tileId, ScenarioGraphNodeKind.Tile, tileLabel, tileRenderType );
				AddEdge( chId, tileId, ScenarioGraphEdgeKind.Contains, "contains" );
				TryTrigger( tile.triggerName, tr => AddEdge( tileId, tr, ScenarioGraphEdgeKind.ExploreTriggers, "explore triggers" ) );

				foreach ( var token in tile.tokenList )
				{
					var tokId = TokenId( token.GUID );
					var tokenRenderType = token.tokenType == TokenType.Terrain ? "token-diamond" : "token-circle";
					var tokenLabel = TokenLabel( token );
					AddNode( tokId, ScenarioGraphNodeKind.Token, tokenLabel, tokenRenderType );
					AddEdge( tileId, tokId, ScenarioGraphEdgeKind.Contains, "contains" );
					TryTrigger( token.triggeredByName, tr => AddEdge( tr, tokId, ScenarioGraphEdgeKind.Activates, "activates" ) );
					TryEvent( token.triggerName, ev => AddEdge( tokId, ev, ScenarioGraphEdgeKind.Triggers, "triggers" ) );
				}
			}

			if ( string.IsNullOrEmpty( ch.triggeredBy ) || ch.triggeredBy == "None" )
				AddEdge( ScenarioGraphDocumentBuilder.StartNodeId, chId, ScenarioGraphEdgeKind.ExploresFromStart, "explores" );

			var grpKey = InteractionGroupIdFromChapterField( ch.randomInteractionGroup );
			if ( grpKey != null && ids.Contains( grpKey ) )
				AddEdge( chId, grpKey, ScenarioGraphEdgeKind.Activates, "activates" );

			TryTrigger( ch.triggeredBy, tr => AddEdge( tr, chId, ScenarioGraphEdgeKind.Reveals, "reveals" ) );
			TryTrigger( ch.triggerName, tr => AddEdge( chId, tr, ScenarioGraphEdgeKind.Triggers, "triggers" ) );
			TryTrigger( ch.exploreTrigger, tr => AddEdge( chId, tr, ScenarioGraphEdgeKind.ExploreTriggers, "explore triggers" ) );
			TryTrigger( ch.exploredAllTilesTrigger, tr => AddEdge( chId, tr, ScenarioGraphEdgeKind.FullyExploredTriggers, "fully explored triggers" ) );
		}

		if ( !scenario.threatNotUsed )
		{
			var prevThreatId = ScenarioGraphDocumentBuilder.StartNodeId;
			foreach ( var th in scenario.threatObserver.OrderBy( x => x.threshold ) )
			{
				var vId = ThreatId( th.dataName + "_" + th.threshold );
				AddNode( vId, ScenarioGraphNodeKind.ThreatLevel, WithHeader( "\nThreat", "Level " + th.threshold ), "threat-node" );
				AddEdge( prevThreatId, vId, ScenarioGraphEdgeKind.ThreatChain, null );
				prevThreatId = vId;
				TryEvent( th.triggerName, ev => AddEdge( vId, ev, ScenarioGraphEdgeKind.Initiates, "initiates" ) );
			}

			var maxId = ThreatId( "MAX THREAT" );
			AddNode( maxId, ScenarioGraphNodeKind.ThreatLevel, WithHeader( "\nThreat", "Max " + scenario.threatMax ), "threat-node" );
			AddEdge( prevThreatId, maxId, ScenarioGraphEdgeKind.ThreatChain, null );
		}

		return new ScenarioGraphDocument( nodes, edges );
	}

	static string? EnsureInteractionGroupFromInteractionName( string dataName, List<ScenarioGraphNode> nodes, HashSet<string> ids )
	{
		if ( !dataName.Contains( "GRP", StringComparison.Ordinal ) )
			return null;
		var trimmed = dataName.Trim();
		var idx = trimmed.IndexOf( "GRP", StringComparison.Ordinal );
		var groupNameAndNumber = trimmed.Substring( idx );
		var id = "INTGRP_" + groupNameAndNumber;
		if ( ids.Contains( id ) )
			return id;
		ids.Add( id );
		nodes.Add( new ScenarioGraphNode( id, ScenarioGraphNodeKind.InteractionGroup, WithHeader( "Event", groupNameAndNumber ), "event-rect" ) );
		return id;
	}

	/// <summary>WPF: <c>getInteractionGroupName(chapter.randomInteractionGroup)</c>.</summary>
	static string? InteractionGroupIdFromChapterField( string? randomInteractionGroup )
	{
		if ( string.IsNullOrWhiteSpace( randomInteractionGroup ) )
			return null;
		return "INTGRP_" + randomInteractionGroup.Trim();
	}

	public static string TriggerId( string dataName ) => "TRIG_" + dataName;
	public static string ObjectiveId( string dataName ) => "OBJ_" + dataName;
	public static string ResolutionId( string dataName ) => "RES_" + dataName;
	public static string ChapterId( string dataName ) => "CHAP_" + dataName;
	public static string TileId( Guid guid ) => "TILE_" + guid.ToString();
	public static string TokenId( Guid guid ) => "TOKEN_" + guid.ToString();
	public static string InteractionId( string dataName ) => "INT_" + dataName;
	public static string ThreatId( string key ) => "Threat_" + key;

	static string TokenLabel( Token token ) =>
		token.tokenType switch
		{
			TokenType.Person => WithHeader( "Token", token.personType == PersonType.None ? "Person" : $"Person\n{token.personType}" ),
			TokenType.Terrain => WithHeader( "Terrain", token.terrainType == TerrainType.None ? "Terrain" : token.terrainType.ToString() ),
			_ => WithHeader( "Token", token.tokenType.ToString() ),
		};

	static string WithHeader( string header, string? name )
	{
		var trimmed = string.IsNullOrWhiteSpace( name ) ? "(unnamed)" : name.Trim();
		return $"{header}\n{trimmed}";
	}
}

