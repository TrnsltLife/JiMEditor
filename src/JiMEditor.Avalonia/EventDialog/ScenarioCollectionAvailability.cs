// JiMEditor Avalonia: scenario collection rules for event editors (not in WPF).
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using JiMEditor.Avalonia.Views;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.EventDialog;

/// <summary>
/// Centralizes which enemies, tokens, items, and titles are valid for the scenario's enabled collections.
/// </summary>
public static class ScenarioCollectionAvailability
{
	public static void ApplyThreatEnemyPool( Scenario scenario, ThreatEnemyPoolTab tab )
	{
		if ( scenario == null )
			return;
		tab.BeginSuppressCollectionUi();
		try
		{
			var map = new (CheckBox Header, StackPanel? IconRow, IReadOnlyList<CheckBox> Monsters, Collection Coll)[]
			{
				( tab.CoreSetCheckBox, tab.CoreSetPoolHeaderPanel, tab.CoreMonsterCheckBoxes, Collection.CORE_SET ),
				( tab.VillainsCheckBox, tab.VillainsPoolHeaderPanel, tab.VillainsMonsterCheckBoxes, Collection.VILLAINS_OF_ERIAJAR ),
				( tab.ShadedPathsCheckBox, tab.ShadedPathsPoolHeaderPanel, tab.ShadedMonsterCheckBoxes, Collection.SHADED_PATHS ),
				( tab.DenizensCheckBox, tab.DenizensPoolHeaderPanel, tab.DenizensMonsterCheckBoxes, Collection.DENIZENS_IN_DARKNESS ),
				( tab.UnfurlingWarCheckBox, tab.UnfurlingWarPoolHeaderPanel, tab.UnfurlingMonsterCheckBoxes, Collection.UNFURLING_WAR ),
				( tab.ScorchersCheckBox, tab.ScorchersPoolHeaderPanel, tab.ScorchersMonsterCheckBoxes, Collection.SCORCHERS_OF_THE_WILDS ),
			};
			foreach ( var (header, iconRow, monsters, coll) in map )
			{
				var enabled = scenario.IsCollectionEnabled( coll );
				if ( enabled )
				{
					header.IsEnabled = true;
					header.FontStyle = FontStyle.Normal;
					foreach ( var cb in monsters )
					{
						cb.IsEnabled = true;
						cb.FontStyle = FontStyle.Normal;
					}
				}
				else
				{
					header.IsChecked = false;
					header.IsEnabled = false;
					header.FontStyle = FontStyle.Italic;
					foreach ( var cb in monsters )
					{
						cb.IsChecked = false;
						cb.IsEnabled = false;
						cb.FontStyle = FontStyle.Italic;
					}
				}

				SetPoolHeaderIconItalic( iconRow, !enabled );
			}
		}
		finally
		{
			tab.EndSuppressCollectionUi();
		}
	}

	static void SetPoolHeaderIconItalic( StackPanel? iconRow, bool italic )
	{
		if ( iconRow == null )
			return;
		var fs = italic ? FontStyle.Italic : FontStyle.Normal;
		foreach ( var c in iconRow.Children )
			if ( c is TextBlock tb )
				tb.FontStyle = fs;
	}

	public static void ApplyScriptedEnemyGroupEditor( Scenario scenario, ScriptedEnemyGroupEditorWindow window )
	{
		var core = new List<RadioButton> { window.RuffianRadio, window.GoblinScoutRadio, window.OrcHunterRadio, window.OrcMarauderRadio, window.HungryVargRadio, window.HillTrollRadio, window.WightRadio };
		var villains = new List<RadioButton> { window.AtariRadio, window.GargletargRadio, window.ChartoothRadio };
		var shaded = new List<RadioButton> { window.GiantSpiderRadio, window.PitGoblinRadio, window.OrcTaskmasterRadio, window.ShadowmanRadio, window.AnonymousThingRadio, window.CaveTrollRadio, window.BalerockRadio, window.SpawnOfUglygiantRadio };
		var denizens = new List<RadioButton> { window.SupplicantRadio, window.UrsulaRadio, window.OliverRadio };
		var unfurling = new List<RadioButton> { window.FoulBeastRadio, window.VargRiderRadio, window.SiegeEngineRadio, window.WarElephantRadio, window.SoldierRadio, window.HighOrcWarriorRadio };
		var scorchers = new List<RadioButton> { window.LordJavelinRadio, window.LichKingRadio, window.EndrisRadio };
		var map = new (List<RadioButton> Rbs, Collection Coll, StackPanel? Header)[]
		{
			( core, Collection.CORE_SET, window.CoreSetEnemyTypeHeaderPanel ),
			( villains, Collection.VILLAINS_OF_ERIAJAR, window.VillainsEnemyTypeHeaderPanel ),
			( shaded, Collection.SHADED_PATHS, window.ShadedPathsEnemyTypeHeaderPanel ),
			( denizens, Collection.DENIZENS_IN_DARKNESS, window.DenizensEnemyTypeHeaderPanel ),
			( unfurling, Collection.UNFURLING_WAR, window.UnfurlingWarEnemyTypeHeaderPanel ),
			( scorchers, Collection.SCORCHERS_OF_THE_WILDS, window.ScorchersEnemyTypeHeaderPanel ),
		};
		foreach ( var (rbs, coll, header) in map )
		{
			if ( scenario.IsCollectionEnabled( coll ) )
			{
				foreach ( var rb in rbs )
				{
					rb.IsEnabled = true;
					rb.FontStyle = FontStyle.Normal;
				}
				SetEnemyTypeHeaderItalic( header, italic: false );
			}
			else
			{
				foreach ( var rb in rbs )
				{
					rb.IsChecked = false;
					rb.IsEnabled = false;
					rb.FontStyle = FontStyle.Italic;
				}
				SetEnemyTypeHeaderItalic( header, italic: true );
			}
		}
	}

	static void SetEnemyTypeHeaderItalic( StackPanel? header, bool italic )
	{
		if ( header == null )
			return;
		var fs = italic ? FontStyle.Italic : FontStyle.Normal;
		foreach ( var c in header.Children )
			if ( c is TextBlock tb )
				tb.FontStyle = fs;
	}

	public static IReadOnlyList<TokenType> GetSelectableTokenTypes( Scenario scenario, bool journeyMap )
	{
		var list = new List<TokenType>();
		foreach ( var tt in SelectableTokenTypesBase )
		{
			if ( tt is TokenType.DifficultGround or TokenType.Fortified )
			{
				var ok = scenario.collectionObserver.Any( c =>
					( tt == TokenType.DifficultGround && c.DifficultGround ) ||
					( tt == TokenType.Fortified && c.Fortified ) );
				if ( !ok )
					continue;
			}

			if ( tt == TokenType.Terrain && journeyMap )
				continue;

			list.Add( tt );
		}

		return list;
	}

	static readonly TokenType[] SelectableTokenTypesBase =
	{
		TokenType.Search, TokenType.Person, TokenType.Threat, TokenType.Darkness,
		TokenType.DifficultGround, TokenType.Fortified, TokenType.Terrain
	};

	/// <summary>Token types allowed for the scenario; invalid selections fall back to <see cref="TokenType.Search"/>.</summary>
	public static void CoerceInteractionTokenForScenario( InteractionBase interaction, Scenario scenario, bool journeyMap )
	{
		if ( interaction.interactionType == InteractionType.Start )
			return;

		var allowed = GetSelectableTokenTypes( scenario, journeyMap );
		if ( !allowed.Contains( interaction.tokenType ) )
			interaction.tokenType = TokenType.Search;

		if ( interaction.tokenType == TokenType.Terrain )
		{
			var terrains = GetSelectableTerrainTypes( scenario );
			if ( terrains.Count == 0 )
				interaction.terrainType = TerrainType.Boulder;
			else if ( !terrains.Contains( interaction.terrainType ) )
				interaction.terrainType = terrains[0];
		}
	}

	public static void CoercePlacedTokenForScenario( Token token, Scenario scenario )
	{
		if ( token.triggerName == StartInteraction.DEFAULT_NAME || token.tokenType == TokenType.Start )
		{
			token.RepairStartPositionFromTriggerName();
			return;
		}

		// Map tokens may be Terrain on Journey scenarios; event token pickers skip Terrain on Journey maps.
		var allowed = GetSelectableTokenTypes( scenario, journeyMap: false );
		if ( !allowed.Contains( token.tokenType ) )
			token.tokenType = TokenType.Search;

		if ( token.tokenType == TokenType.Terrain )
		{
			var terrains = GetSelectableTerrainTypes( scenario );
			if ( terrains.Count == 0 )
				token.tokenType = TokenType.Search;
			else if ( !terrains.Contains( token.terrainType ) )
				token.terrainType = terrains[0];
		}

		token.UpdateDataName();
	}

	public static IReadOnlyList<TerrainType> GetSelectableTerrainTypes( Scenario scenario )
	{
		var list = new List<TerrainType>();
		foreach ( var t in Enum.GetValues<TerrainType>() )
		{
			if ( t == TerrainType.None )
				continue;
			var coll = Collection.FromTerrainType( t );
			if ( coll != null && scenario.collectionObserver.Contains( coll ) )
				list.Add( t );
		}
		return list;
	}

	public static void PruneItemInteraction( ItemInteraction interaction, Scenario scenario )
	{
		var toRemove = interaction.itemList
			.Where( it => !scenario.collectionObserver.Contains( Collection.FromID( it.collection ) ) )
			.ToList();
		foreach ( var it in toRemove )
			interaction.itemList.Remove( it );

		var count = interaction.itemList.Count;
		if ( interaction.randomizedItemsCount > count && count > 0 )
			interaction.randomizedItemsCount = count;
		if ( interaction.randomizedItemsCount < 1 )
			interaction.randomizedItemsCount = 1;
		if ( count == 0 )
			interaction.randomizedItemsCount = 1;
	}

	public static void PruneTitleInteraction( TitleInteraction interaction, Scenario scenario )
	{
		var toRemove = interaction.titleList
			.Where( t => !scenario.collectionObserver.Contains( Collection.FromID( t.collection ) ) )
			.ToList();
		foreach ( var t in toRemove )
			interaction.titleList.Remove( t );

		var count = interaction.titleList.Count;
		if ( interaction.randomizedTitlesCount > count && count > 0 )
			interaction.randomizedTitlesCount = count;
		if ( interaction.randomizedTitlesCount < 1 )
			interaction.randomizedTitlesCount = 1;
		if ( count == 0 )
			interaction.randomizedTitlesCount = 1;
	}

	/// <summary>
	/// Clears enemy / token / item / title picks that belong to collections not enabled on the scenario, including inside
	/// events that were never opened in the editor. Also removes placed tiles from disabled collections and coerces
	/// tokens that those collections no longer allow. Safe to call repeatedly.
	/// </summary>
	public static void PruneScenarioForEnabledCollections( Scenario scenario )
	{
		if ( scenario == null )
			return;
		var journeyMap = scenario.scenarioTypeJourney;
		foreach ( var inter in scenario.interactionObserver )
		{
			switch ( inter )
			{
				case ItemInteraction item:
					PruneItemInteraction( item, scenario );
					break;
				case TitleInteraction title:
					PruneTitleInteraction( title, scenario );
					break;
				case ThreatInteraction threat:
					PruneThreatInteraction( threat, scenario );
					break;
			}

			if ( inter is InteractionBase ib )
				CoerceInteractionTokenForScenario( ib, scenario, journeyMap );
		}

		foreach ( var chapter in scenario.chapterObserver )
		{
			var tilesToRemove = chapter.tileObserver
				.Where( tile =>
				{
					var coll = Collection.FromTileNumber( tile.idNumber );
					return coll != null && !scenario.collectionObserver.Contains( coll );
				} )
				.ToList();
			foreach ( var tile in tilesToRemove )
				chapter.tileObserver.Remove( tile );

			foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
			{
				foreach ( var token in tile.tokenList )
					CoercePlacedTokenForScenario( token, scenario );
			}
		}

		scenario.RefilterGlobalTilePool();
		scenario.RepairStartPositionData();
	}

	public static void PruneThreatInteraction( ThreatInteraction threat, Scenario scenario )
	{
		var monsters = Collection.MONSTERS();
		var flags = threat.includedEnemies;
		for ( var i = 0; i < flags.Length && i < monsters.Length; i++ )
		{
			if ( !flags[i] )
				continue;
			var coll = CollectionForMonsterTypeId( monsters[i].id );
			if ( coll == null || !scenario.collectionObserver.Contains( coll ) )
				flags[i] = false;
		}

		var toRemove = threat.monsterCollection
			.Where( m =>
			{
				var c = CollectionForMonsterTypeId( m.id );
				return c == null || !scenario.collectionObserver.Contains( c );
			} )
			.ToList();
		foreach ( var m in toRemove )
			threat.monsterCollection.Remove( m );
	}

	static Collection? CollectionForMonsterTypeId( int monsterTypeId ) =>
		Collection.FromMonsterTypeId( monsterTypeId );
}
