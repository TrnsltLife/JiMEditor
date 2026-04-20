using System.Linq;
using System.Threading;
using System.Windows;
using Avalonia;
using JiME;
using JiMEditor.Avalonia;
using Newtonsoft.Json;
using Xunit;
using Xunit.Sdk;

namespace JiMEditor.Avalonia.Tests;

public class TileTokenSerializationRoundTripTests
{
	static readonly object AvaloniaInitLock = new();
	static bool AvaloniaInitialized;

	[Fact]
	public void JourneyTile_RoundTrip_PreservesNumericState()
	{
		RunInSta( () =>
		{
			SetupMinimalUtilsForModelTests();

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
		RunInSta( () =>
		{
			SetupMinimalUtilsForModelTests();

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
		RunInSta( () =>
		{
			SetupMinimalUtilsForModelTests();

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
		RunInSta( () =>
		{
			SetupMinimalUtilsForModelTests();

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
		RunInSta( () =>
		{
			SetupMinimalUtilsForModelTests();

			var scenario = new Scenario( "Duplicate Trigger Scenario" );
			scenario.triggersObserver.Add( new JiME.Trigger( "OldTrigger" ) );
			scenario.triggersObserver.Add( new JiME.Trigger( "ExistingTrigger" ) );

			var renamed = scenario.RenameTrigger( "OldTrigger", "ExistingTrigger", isMulti: false );

			Assert.False( renamed );
			Assert.Contains( scenario.triggersObserver, t => t.dataName == "OldTrigger" );
			Assert.Contains( scenario.triggersObserver, t => t.dataName == "ExistingTrigger" );
		} );
	}

	static void RunInSta( Action action )
	{
		Exception? threadException = null;
		var thread = new Thread( () =>
		{
			try
			{
				action();
			}
			catch ( Exception ex )
			{
				threadException = ex;
			}
		} );
		thread.SetApartmentState( ApartmentState.STA );
		thread.Start();
		thread.Join();
		if ( threadException != null )
			throw new XunitException( threadException.ToString() );
	}

	static void SetupMinimalUtilsForModelTests()
	{
		EnsureAvaloniaInitialized();
		Utils.dragSnapX = new[] { 0d, 48d, 96d, 144d, 192d, 240d };
		Utils.dragSnapY = new[] { 0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d };
		Utils.defaultStats = [];
		Utils.defaultActivations = [];
		Utils.defaultTerrainInteractions = [];
	}

	static void EnsureAvaloniaInitialized()
	{
		if ( AvaloniaInitialized )
			return;
		lock ( AvaloniaInitLock )
		{
			if ( AvaloniaInitialized )
				return;
			AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.SetupWithoutStarting();
			AvaloniaInitialized = true;
		}
	}
}
