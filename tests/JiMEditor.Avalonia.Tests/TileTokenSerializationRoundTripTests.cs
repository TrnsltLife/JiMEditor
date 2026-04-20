using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using JiME;
using Newtonsoft.Json;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class TileTokenSerializationRoundTripTests
{
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
				position = new System.Windows.Vector( 336, 249.4153152 ),
				angle = 105,
				triggerName = "TileExploredTrigger",
				isStartTile = true
			};
			chapter.AddTile( tile );

			var fm = new FileManager( scenario );
			var json = JsonConvert.SerializeObject( fm );
			var deserializedFileManager = JsonConvert.DeserializeObject<FileManager>( json );
			Assert.NotNull( deserializedFileManager );

			var roundTrippedScenario = Scenario.CreateInstance( deserializedFileManager! );
			var roundTrippedChapter = roundTrippedScenario.chapterObserver.Single( c => c.dataName == "Chapter A" );
			var roundTrippedTile = Assert.IsType<HexTile>( roundTrippedChapter.tileObserver.Single() );

			Assert.Equal( 213, roundTrippedTile.idNumber );
			Assert.Equal( "B", roundTrippedTile.tileSide );
			Assert.Equal( 336, roundTrippedTile.position.X, 6 );
			Assert.Equal( 249.4153152, roundTrippedTile.position.Y, 6 );
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
				position = new System.Windows.Vector( 128.5, 384.25 ),
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
