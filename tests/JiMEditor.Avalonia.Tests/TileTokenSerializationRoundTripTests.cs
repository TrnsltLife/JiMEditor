using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using JiME;
using Newtonsoft.Json;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class TileTokenSerializationRoundTripTests
{
	[Fact]
	public void Vector2_DeserializesWpfCommaSeparatedString()
	{
		var v = JsonConvert.DeserializeObject<Vector2>( "\"0,1\"" );
		Assert.Equal( 0, v.X );
		Assert.Equal( 1, v.Y );

		v = JsonConvert.DeserializeObject<Vector2>( "\"336,249.4153152\"" );
		Assert.Equal( 336, v.X, 6 );
		Assert.Equal( 249.4153152, v.Y, 6 );
	}

	[Fact]
	public void Vector2_SerializesAsWpfCommaSeparatedString()
	{
		var json = JsonConvert.SerializeObject( new Vector2( 0, 1 ) );
		Assert.Equal( "\"0,1\"", json );
	}

	[Fact]
	public void Vector2_DeserializesObjectAndArrayShapes()
	{
		var fromObj = JsonConvert.DeserializeObject<Vector2>( "{\"X\":2,\"Y\":3}" );
		Assert.Equal( 2, fromObj.X );
		Assert.Equal( 3, fromObj.Y );

		var fromArr = JsonConvert.DeserializeObject<Vector2>( "[4.5,6.25]" );
		Assert.Equal( 4.5, fromArr.X );
		Assert.Equal( 6.25, fromArr.Y );
	}

	[Fact]
	public void HexTile_LegacyPathRootString_Deserializes()
	{
		// Minimal tile JSON shape as written by WPF (pathRoot / position are "X,Y" strings).
		const string json = """
			{
			  "tileType": 0,
			  "idNumber": 213,
			  "tileSide": "A",
			  "position": "336,249.4153152",
			  "pathRoot": "0,1",
			  "angle": 0,
			  "tokenCount": 1,
			  "GUID": "00000000-0000-0000-0000-000000000001",
			  "isStartTile": false,
			  "triggerName": "None",
			  "tokenList": [],
			  "color": 0
			}
			""";

		var tile = JsonConvert.DeserializeObject<HexTile>( json );
		Assert.NotNull( tile );
		Assert.Equal( 0, tile!.pathRoot.X );
		Assert.Equal( 1, tile.pathRoot.Y );
		Assert.Equal( 336, tile.position.X, 6 );
		Assert.Equal( 249.4153152, tile.position.Y, 6 );
	}

	[Fact]
	public void JourneyTile_RoundTrip_PreservesNumericState()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "RoundTrip Scenario" );
			var chapter = new Chapter( "Chapter A" );
			scenario.chapterObserver.Add( chapter );

			var tile = new HexTile( 213, skipBuild: true )
			{
				tileSide = "B",
				position = new Vector2( 336, 249.4153152 ),
				pathRoot = new Vector2( 0, 1 ),
				angle = 105,
				triggerName = "TileExploredTrigger",
				isStartTile = true
			};
			chapter.AddTile( tile );

			var fm = new FileManager( scenario );
			var json = JsonConvert.SerializeObject( fm, Formatting.Indented );
			// WPF Vector TypeConverter wrote "X,Y" strings — keep that on-disk shape.
			Assert.Contains( "\"0,1\"", json );
			Assert.Contains( "\"336,249.4153152\"", json );

			var deserializedFileManager = JsonConvert.DeserializeObject<FileManager>( json );
			Assert.NotNull( deserializedFileManager );

			var roundTrippedScenario = Scenario.CreateInstance( deserializedFileManager! );
			var roundTrippedChapter = roundTrippedScenario.chapterObserver.Single( c => c.dataName == "Chapter A" );
			var roundTrippedTile = Assert.IsType<HexTile>( roundTrippedChapter.tileObserver.Single() );

			Assert.Equal( 213, roundTrippedTile.idNumber );
			Assert.Equal( "B", roundTrippedTile.tileSide );
			Assert.Equal( 336, roundTrippedTile.position.X, 6 );
			Assert.Equal( 249.4153152, roundTrippedTile.position.Y, 6 );
			Assert.Equal( 0, roundTrippedTile.pathRoot.X );
			Assert.Equal( 1, roundTrippedTile.pathRoot.Y );
			Assert.Equal( 105, roundTrippedTile.angle, 6 );
			Assert.Equal( "TileExploredTrigger", roundTrippedTile.triggerName );
			Assert.True( roundTrippedTile.isStartTile );
		} );
	}

	[Fact]
	public void JourneyTile_TranslationCollection_IncludesTileExploredText()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Translation Scenario" );
			var chapter = new Chapter( "Chapter A" );
			scenario.chapterObserver.Add( chapter );

			var tile = new HexTile( 213, skipBuild: true );
			tile.flavorBookData.pages[0] = "Tile explored flavor";
			chapter.AddTile( tile );

			var items = scenario.CollectAllTranslationItems();
			var tileEntry = items.SingleOrDefault( it => it.key == "tile.213.exploredText" );

			Assert.NotNull( tileEntry );
			Assert.Equal( "Tile explored flavor", tileEntry!.text );
		} );
	}

	[Fact]
	public void StartInteraction_Deserialize_RestoresTokenTypeStart()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			const string json = """
				{
				  "interactionType": 14,
				  "dataName": "Starting Position",
				  "tokenType": 0,
				  "isTokenInteraction": false
				}
				""";

			var start = JsonConvert.DeserializeObject<StartInteraction>( json );
			Assert.NotNull( start );
			start!.ApplyStartInteractionDefaults();

			Assert.Equal( TokenType.Start, start.tokenType );
			Assert.True( start.isTokenInteraction );
			Assert.Equal( InteractionType.Start, start.interactionType );
		} );
	}

	[Fact]
	public void StartPositionToken_ApplyInteraction_SetsStartTokenType()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var start = new StartInteraction();
			start.tokenType = TokenType.Search;

			var token = new Token( TokenType.None );
			token.ApplyInteraction( start );

			Assert.Equal( StartInteraction.DEFAULT_NAME, token.triggerName );
			Assert.Equal( TokenType.Start, token.tokenType );
			Assert.Equal( "Start", token.dataName );
		} );
	}

	[Fact]
	public void StartPositionToken_RoundTrip_PreservesStartTokenType()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Start Token Scenario" );
			var chapter = new Chapter( "Chapter A" );
			scenario.chapterObserver.Add( chapter );

			var tile = new HexTile( 213, skipBuild: true ) { isStartTile = true };
			var token = new Token( TokenType.Start )
			{
				triggerName = StartInteraction.DEFAULT_NAME,
				position = new Vector2( 200, 220 )
			};
			tile.tokenList.Add( token );
			chapter.AddTile( tile );

			var fm = new FileManager( scenario );
			var json = JsonConvert.SerializeObject( fm );
			var deserializedFileManager = JsonConvert.DeserializeObject<FileManager>( json );
			Assert.NotNull( deserializedFileManager );

			var roundTrippedScenario = Scenario.CreateInstance( deserializedFileManager! );
			var roundTrippedTile = Assert.IsType<HexTile>(
				roundTrippedScenario.chapterObserver.Single().tileObserver.Single() );
			var roundTrippedToken = roundTrippedTile.tokenList.Single();

			Assert.Equal( StartInteraction.DEFAULT_NAME, roundTrippedToken.triggerName );
			Assert.Equal( TokenType.Start, roundTrippedToken.tokenType );

			var startInteraction = roundTrippedScenario.interactionObserver
				.First( i => i.dataName == StartInteraction.DEFAULT_NAME );
			Assert.Equal( TokenType.Start, startInteraction.tokenType );
		} );
	}

	[Fact]
	public void CreateInstance_RepairsStartPositionToken_WhenTokenTypeWasSearch()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Repair Start Scenario" );
			var chapter = new Chapter( "Chapter A" );
			scenario.chapterObserver.Add( chapter );

			var tile = new HexTile( 213, skipBuild: true ) { isStartTile = true };
			tile.tokenList.Add( new Token( TokenType.Search )
			{
				triggerName = StartInteraction.DEFAULT_NAME
			} );
			chapter.AddTile( tile );

			var fm = new FileManager( scenario );
			var loaded = Scenario.CreateInstance( fm );
			var loadedTile = Assert.IsType<HexTile>( loaded.chapterObserver.Single().tileObserver.Single() );
			var loadedToken = loadedTile.tokenList.Single();

			Assert.Equal( TokenType.Start, loadedToken.tokenType );
			Assert.Equal( "Start", loadedToken.dataName );
		} );
	}

	[Fact]
	public void JourneyTileToken_RoundTrip_PreservesTokenNumericState()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Token RoundTrip Scenario" );
			var chapter = new Chapter( "Chapter A" );
			scenario.chapterObserver.Add( chapter );

			var tile = new HexTile( 213, skipBuild: true );
			var token = new Token( TokenType.Threat )
			{
				position = new Vector2( 128.5, 384.25 ),
				angle = 45,
				triggerName = "TokenReveal",
				triggeredByName = "TokenTriggerBy"
			};
			tile.tokenList.Add( token );
			chapter.AddTile( tile );

			var fm = new FileManager( scenario );
			var json = JsonConvert.SerializeObject( fm );
			var deserializedFileManager = JsonConvert.DeserializeObject<FileManager>( json );
			Assert.NotNull( deserializedFileManager );

			var roundTrippedScenario = Scenario.CreateInstance( deserializedFileManager! );
			var roundTrippedChapter = roundTrippedScenario.chapterObserver.Single( c => c.dataName == "Chapter A" );
			var roundTrippedTile = Assert.IsType<HexTile>( roundTrippedChapter.tileObserver.Single() );
			var roundTrippedToken = roundTrippedTile.tokenList.Single();

			Assert.Equal( TokenType.Threat, roundTrippedToken.tokenType );
			Assert.Equal( 128.5, roundTrippedToken.position.X, 6 );
			Assert.Equal( 384.25, roundTrippedToken.position.Y, 6 );
			Assert.Equal( 45, roundTrippedToken.angle, 6 );
			Assert.Equal( "TokenReveal", roundTrippedToken.triggerName );
			Assert.Equal( "TokenTriggerBy", roundTrippedToken.triggeredByName );
		} );
	}

	[Fact]
	public void Save_PrunesCollectionLockedContent_FromScenarioModel_BeforeWrite()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Prune Save Scenario" );
			scenario.collectionObserver.Clear();
			scenario.collectionObserver.Add( Collection.CORE_SET );
			scenario.globalTilePool.Clear();
			scenario.globalTilePool.Add( 100 ); // Core tile
			scenario.globalTilePool.Add( 213 ); // Shaded Paths tile

			var itemInteraction = new ItemInteraction( "Item Event" )
			{
				randomizedItemsCount = 2
			};
			itemInteraction.itemList.Add( Items.FromID( 1 )! ); // Core
			itemInteraction.itemList.Add( Items.FromID( 26 )! ); // Shaded Paths
			scenario.interactionObserver.Add( itemInteraction );

			var titleInteraction = new TitleInteraction( "Title Event" )
			{
				randomizedTitlesCount = 2
			};
			titleInteraction.titleList.Add( Titles.FromID( 1 )! ); // Core
			titleInteraction.titleList.Add( Titles.FromID( 25 )! ); // Shaded Paths
			scenario.interactionObserver.Add( titleInteraction );

			var threat = new ThreatInteraction( "Threat Event" );
			threat.includedEnemies[0] = true; // Core
			threat.includedEnemies[10] = true; // Shaded Paths
			threat.monsterCollection.Add( new Monster( 0 ) ); // Core
			threat.monsterCollection.Add( new Monster( 10 ) ); // Shaded Paths
			scenario.interactionObserver.Add( threat );

			var tokenInteraction = new ItemInteraction( "Token Event" )
			{
				isTokenInteraction = true,
				tokenType = TokenType.Terrain,
				terrainType = TerrainType.Log // Shaded Paths terrain
			};
			scenario.interactionObserver.Add( tokenInteraction );

			var outPath = Path.Combine( Path.GetTempPath(), "jime-prune-save-" + Guid.NewGuid() + ".jime" );
			try
			{
				var ok = FileManager.TryWriteScenarioToPath( scenario, outPath, updateScenarioAfterWrite: false, out var err );
				Assert.True( ok, err );

				Assert.Single( itemInteraction.itemList );
				Assert.All( itemInteraction.itemList, i => Assert.Equal( Collection.CORE_SET, Collection.FromID( i.collection ) ) );

				Assert.Single( titleInteraction.titleList );
				Assert.All( titleInteraction.titleList, t => Assert.Equal( Collection.CORE_SET, Collection.FromID( t.collection ) ) );

				Assert.True( threat.includedEnemies[0] );
				Assert.False( threat.includedEnemies[10] );
				Assert.Single( threat.monsterCollection );
				Assert.Contains( threat.monsterCollection, m => m.id == 0 );
				Assert.DoesNotContain( threat.monsterCollection, m => m.id == 10 );

				Assert.Equal( TokenType.Search, tokenInteraction.tokenType );
				Assert.DoesNotContain( scenario.filteredGlobalTilePool, x => x == 213 );
				Assert.Contains( 100, scenario.filteredGlobalTilePool );
			}
			finally
			{
				if ( File.Exists( outPath ) )
					File.Delete( outPath );
			}
		} );
	}

	[Fact]
	public void RenameTrigger_UpdatesScenarioReferences_AndTriggerMetadata()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Rename Trigger Scenario" );
			var objective = new Objective( "Obj" ) { triggerName = "OldTrigger", nextTrigger = "OldTrigger", triggeredByName = "OldTrigger" };
			var threat = new Threat( "Threat" ) { triggerName = "OldTrigger" };
			var chapter = new Chapter( "Chapter A" ) { triggerName = "OldTrigger", triggeredBy = "OldTrigger", exploreTrigger = "OldTrigger" };
			var trigger = new JiME.Trigger( "OldTrigger" );
			scenario.objectiveObserver.Add( objective );
			scenario.threatObserver.Add( threat );
			scenario.chapterObserver.Add( chapter );
			scenario.triggersObserver.Add( trigger );

			var renamed = scenario.RenameTrigger( "OldTrigger", "RenamedTrigger", isMulti: true );

			Assert.True( renamed );
			Assert.Equal( "RenamedTrigger", objective.triggerName );
			Assert.Equal( "RenamedTrigger", objective.nextTrigger );
			Assert.Equal( "RenamedTrigger", objective.triggeredByName );
			Assert.Equal( "RenamedTrigger", threat.triggerName );
			Assert.Equal( "RenamedTrigger", chapter.triggerName );
			Assert.Equal( "RenamedTrigger", chapter.triggeredBy );
			Assert.Equal( "RenamedTrigger", chapter.exploreTrigger );
			Assert.Contains( scenario.triggersObserver, t => t.dataName == "RenamedTrigger" && t.isMultiTrigger );
			Assert.DoesNotContain( scenario.triggersObserver, t => t.dataName == "OldTrigger" );
		} );
	}

	[Fact]
	public void RenameTrigger_ReturnsFalse_WhenNewNameAlreadyExists()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Duplicate Trigger Scenario" );
			scenario.triggersObserver.Add( new JiME.Trigger( "OldTrigger" ) );
			scenario.triggersObserver.Add( new JiME.Trigger( "ExistingTrigger" ) );

			var renamed = scenario.RenameTrigger( "OldTrigger", "ExistingTrigger", isMulti: false );

			Assert.False( renamed );
			Assert.Contains( scenario.triggersObserver, t => t.dataName == "OldTrigger" );
			Assert.Contains( scenario.triggersObserver, t => t.dataName == "ExistingTrigger" );
		} );
	}

	[Fact]
	public void ChapterTranslationTail_RenamesTranslationKeys_WhenChapterNameChanges()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var chapter = new Chapter( "OldChapter" )
			{
				noFlavorText = false
			};
			chapter.flavorBookData.pages[0] = "Chapter flavor";

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "chapter.OldChapter.exploredText", "Translated chapter flavor", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = chapter.CaptureStartingValues();
			var originalPrefix = chapter.TranslationKeyPrefix();
			var originalKeyName = chapter.TranslationKeyName();

			chapter.dataName = "RenamedChapter";
			chapter.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			Assert.DoesNotContain( translation.translationItems, i => i.key == "chapter.OldChapter.exploredText" );
			Assert.Contains( translation.translationItems, i => i.key == "chapter.RenamedChapter.exploredText" );
		} );
	}

	[Fact]
	public void ChapterTranslationTail_DecertifiesUpdatedFlavorText()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var chapter = new Chapter( "ChapterA" )
			{
				noFlavorText = false
			};
			chapter.flavorBookData.pages[0] = "Original flavor";

			var translation = new Translation( "en" )
			{
				translationItems = new ObservableCollection<TranslationItem>
				{
					new( "chapter.ChapterA.exploredText", "Translated chapter flavor", translationOK: true )
				}
			};
			var translations = new ObservableCollection<Translation> { translation };

			var originalValues = chapter.CaptureStartingValues();
			var originalPrefix = chapter.TranslationKeyPrefix();
			var originalKeyName = chapter.TranslationKeyName();

			chapter.flavorBookData.pages[0] = "Updated flavor";
			chapter.ApplyTranslationUpdatesAfterEditorDialog( translations, originalValues, originalPrefix, originalKeyName );

			var translatedItem = translation.translationItems.Single( i => i.key == "chapter.ChapterA.exploredText" );
			Assert.False( translatedItem.translationOK );
		} );
	}
}
