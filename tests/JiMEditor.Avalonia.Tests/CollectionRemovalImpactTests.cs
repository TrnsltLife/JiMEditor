using JiME;
using JiMEditor.Avalonia.EventDialog;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class CollectionRemovalImpactTests
{
	static void SeedSnapGrids()
	{
		if ( Utils.dragSnapX == null || Utils.dragSnapX.Length < 6 )
		{
			Utils.dragSnapX = [0d, 48d, 96d, 144d, 192d, 240d];
			Utils.dragSnapY = [0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d];
		}
	}

	[Fact]
	public void GetImpactForRemovingCollection_IncludesPlacedTilesAndTokens()
	{
		SeedSnapGrids();
		var scenario = new Scenario( "Impact" );
		scenario.collectionObserver.Clear();
		scenario.collectionObserver.Add( Collection.CORE_SET );
		scenario.collectionObserver.Add( Collection.SHADED_PATHS );

		var chapter = new Chapter( "Start" );
		var shadedTile = new HexTile( 213, skipBuild: true );
		shadedTile.tokenList.Add( new Token( TokenType.Terrain ) { terrainType = TerrainType.Log } );
		chapter.tileObserver.Add( shadedTile );
		var coreTile = new HexTile( 100, skipBuild: true );
		coreTile.tokenList.Add( new Token( TokenType.Terrain ) { terrainType = TerrainType.Log } );
		chapter.tileObserver.Add( coreTile );
		scenario.chapterObserver.Add( chapter );

		var impact = ScenarioCollectionRemovalImpact.GetImpactForRemovingCollection( scenario, Collection.SHADED_PATHS );

		Assert.Equal( 1, impact.Tiles["213"] );
		Assert.False( impact.Tiles.ContainsKey( "100" ) );
		Assert.Equal( 2, impact.Tokens["Terrain: Log"] );

		var formatted = CollectionRemovalImpact.FormatForDialog( impact );
		Assert.Contains( "Tiles: 213", formatted );
		Assert.Contains( "Tokens: Terrain: Log (2)", formatted );
	}
}
