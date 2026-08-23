using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class ErrorCheckerTests
{
	static void SeedSnapGrids()
	{
		// Avoid Avalonia AppBuilder (native dispatcher) — ErrorChecker only needs Scenario/HexTile data.
		if ( Utils.dragSnapX == null || Utils.dragSnapX.Length < 6 )
		{
			Utils.dragSnapX = [0d, 48d, 96d, 144d, 192d, 240d];
			Utils.dragSnapY = [0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d];
		}
	}

	static Scenario CheckScenario()
	{
		SeedSnapGrids();
		return new Scenario( "Check" );
	}

	/// <summary>
	/// Replaces stock player-facing placeholders so existing tests stay focused on the warning they care about.
	/// </summary>
	static void UseCustomPlayerText( Scenario scenario )
	{
		if ( scenario.introBookData?.pages != null && scenario.introBookData.pages.Count > 0 )
			scenario.introBookData.pages[0] = "Custom introduction.";
		foreach ( var resolution in scenario.resolutionObserver )
		{
			if ( resolution.isEmpty || resolution.pages == null || resolution.pages.Count == 0 )
				continue;
			resolution.pages[0] = "Custom resolution.";
		}

		foreach ( var objective in scenario.objectiveObserver )
		{
			if ( objective.isEmpty || objective.dataName == "None" )
				continue;
			objective.objectiveReminder = "Do the thing.";
			if ( objective.textBookData?.pages != null && objective.textBookData.pages.Count > 0 )
				objective.textBookData.pages[0] = "Summary of the thing.";
		}

		foreach ( var chapter in scenario.chapterObserver )
		{
			if ( chapter.flavorBookData?.pages.Count > 0
			     && chapter.flavorBookData.pages[0].StartsWith( "This optional text is shown", StringComparison.Ordinal ) )
				chapter.flavorBookData.pages[0] = "The path opens.";
		}

		foreach ( var interaction in scenario.interactionObserver )
		{
			if ( interaction is not InteractionBase ib
			     || interaction is ICommonData { isEmpty: true }
			     || interaction.dataName == "None" )
				continue;
			if ( ib.textBookData?.pages.Count > 0
			     && ib.textBookData.pages[0].StartsWith( "Default Flavor Text", StringComparison.Ordinal ) )
				ib.textBookData.pages[0] = "Custom flavor.";
			if ( ib.eventBookData?.pages.Count > 0
			     && ib.eventBookData.pages[0].StartsWith( "Default Event Text", StringComparison.Ordinal ) )
				ib.eventBookData.pages[0] = "Custom event.";
			switch ( interaction )
			{
				case TestInteraction test:
					if ( test.passBookData.pages.Count > 0 )
						test.passBookData.pages[0] = "You pass.";
					if ( test.failBookData.pages.Count > 0 )
						test.failBookData.pages[0] = "You fail.";
					if ( test.progressBookData.pages.Count > 0 )
						test.progressBookData.pages[0] = "Keep going.";
					break;
				case PersistentTokenInteraction persistent:
					if ( persistent.alternativeBookData.pages.Count > 0 )
						persistent.alternativeBookData.pages[0] = "Custom alternative flavor.";
					break;
				case DecisionInteraction decision:
					decision.choice1 = "Left path";
					decision.choice2 = "Right path";
					decision.choice3 = "Turn back";
					break;
				case DialogInteraction dialog:
					dialog.choice1 = "Ask about the road";
					dialog.choice2 = "Ask about the inn";
					dialog.choice3 = "Say nothing";
					break;
			}
		}

		foreach ( var group in scenario.activationsObserver )
		{
			if ( group.id < MonsterActivations.START_OF_CUSTOM_ACTIVATIONS )
				continue;
			foreach ( var item in group.activations )
			{
				if ( item.text != null && item.text.StartsWith( "New attack text.", StringComparison.Ordinal ) )
					item.text = "The enemy strikes.";
				if ( item.effect != null && item.effect.StartsWith( "New after effect (optional).", StringComparison.Ordinal ) )
					item.effect = "";
			}
		}
	}

	static ErrorChecker RunCheck( Scenario scenario )
	{
		UseCustomPlayerText( scenario );
		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );
		return checker;
	}

	[Fact]
	public void CheckErrors_ReportsDuplicateEventNames()
	{
		var scenario = CheckScenario();
		scenario.interactionObserver.Add( new TextInteraction( "Same Name" ) );
		scenario.interactionObserver.Add( new TextInteraction( "Same Name" ) );

		var checker = RunCheck( scenario );

		Assert.Equal( "Warnings (1):", checker.errorCount );
		Assert.Contains( "Duplicate Event name \"Same Name\".", checker.Errors );
	}

	[Fact]
	public void CheckErrors_IgnoresDuplicateThreatNames_AndReportsDuplicateThresholds()
	{
		var scenario = CheckScenario();
		scenario.threatObserver.Add( new Threat( "Default Name", thresholdValue: 5 ) );
		scenario.threatObserver.Add( new Threat( "Default Name", thresholdValue: 5 ) );
		scenario.threatObserver.Add( new Threat( "Default Name", thresholdValue: 10 ) );

		var checker = RunCheck( scenario );

		Assert.DoesNotContain( "Duplicate Threat name", checker.Errors );
		Assert.Contains( "Duplicate Threat Threshold 5.", checker.Errors );
		Assert.DoesNotContain( "Threshold 10", checker.Errors );
		Assert.Equal( "Warnings (1):", checker.errorCount );

		var warning = Assert.Single( checker.WarningItems );
		Assert.Contains( warning.Spans, s => s.Nav?.Kind == ErrorCheckTargetKind.Threat );
	}

	[Fact]
	public void CheckErrors_SkipsDuplicateThreatThresholdsWhenThreatIsUnused()
	{
		var scenario = CheckScenario();
		scenario.threatNotUsed = true;
		scenario.threatObserver.Add( new Threat( "Default Name", thresholdValue: 0 ) );
		scenario.threatObserver.Add( new Threat( "Default Name", thresholdValue: 0 ) );

		var checker = RunCheck( scenario );

		Assert.Equal( "None Found", checker.Errors );
	}

	[Fact]
	public void CheckErrors_AllowsSameNameOnTriggerAndObjectiveOrTileBlock()
	{
		var scenario = CheckScenario();
		scenario.triggersObserver.Add( new Trigger( "Shared" ) );
		scenario.objectiveObserver.Add( new Objective( "Shared" ) );
		var chapter = new Chapter( "Shared" );
		chapter.tileObserver.Add( new HexTile( 200, skipBuild: true ) );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.DoesNotContain( "used as both", checker.Errors );
		Assert.Equal( "None Found", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsTokenPointingAtMissingAndNonTokenEvents()
	{
		var scenario = CheckScenario();
		var flavorOnly = new TextInteraction( "Flavor Only" );
		flavorOnly.isTokenInteraction = false;
		scenario.interactionObserver.Add( flavorOnly );

		var chapter = new Chapter( "Start" );
		var tile = new HexTile( 200, skipBuild: true );
		tile.tokenList.Add( new Token( TokenType.Search ) { triggerName = "Missing Event" } );
		tile.tokenList.Add( new Token( TokenType.Search ) { triggerName = "Flavor Only" } );
		chapter.tileObserver.Add( tile );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.Contains( "points at missing event \"Missing Event\".", checker.Errors );
		Assert.Contains( "which is not a Token Interaction.", checker.Errors );
	}

	[Fact]
	public void CheckErrors_IgnoresNonePlaceholders()
	{
		var scenario = CheckScenario();
		scenario.triggersObserver.Add( Trigger.EmptyTrigger() );
		scenario.interactionObserver.Add( NoneInteraction.EmptyInteraction() );
		scenario.objectiveObserver.Add( Objective.EmptyObjective() );

		var checker = RunCheck( scenario );

		Assert.Equal( "None Found", checker.Errors );
		Assert.Equal( "Warnings (0):", checker.errorCount );
	}

	[Fact]
	public void CheckErrors_ReportsEmptyTileBlock()
	{
		var scenario = CheckScenario();
		scenario.chapterObserver.Add( new Chapter( "Start" ) );

		var checker = RunCheck( scenario );

		Assert.Equal( "Warnings (1):", checker.errorCount );
		Assert.Contains( "Tile block \"Start\" has no tiles.", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsMissingEventTriggersAndTargetEvents()
	{
		var scenario = CheckScenario();
		var text = new TextInteraction( "Flavor" );
		text.triggerName = "Missing Before";
		text.triggerAfterName = "Missing After";
		scenario.interactionObserver.Add( text );

		var flavorOnly = new TextInteraction( "Flavor Only" );
		flavorOnly.isTokenInteraction = false;
		scenario.interactionObserver.Add( flavorOnly );

		var persistent = new PersistentTokenInteraction( "Persistent" );
		persistent.eventToActivate = "Flavor Only";
		scenario.interactionObserver.Add( persistent );

		var checker = RunCheck( scenario );

		Assert.Contains( "Event \"Flavor\" Triggered By points at missing trigger \"Missing Before\".", checker.Errors );
		Assert.Contains( "Event \"Flavor\" Set Trigger After points at missing trigger \"Missing After\".", checker.Errors );
		Assert.Contains( "Event \"Persistent\" Delegate Event points at event \"Flavor Only\" which is not a Token Interaction.", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsPersistentEventWithNoDelegate()
	{
		var scenario = CheckScenario();
		scenario.interactionObserver.Add( new PersistentTokenInteraction( "Relay" ) );

		var checker = RunCheck( scenario );

		Assert.Contains( "Event \"Relay\" has no Delegate Event.", checker.Errors );
		var warning = Assert.Single( checker.WarningItems, w => w.Text.Contains( "Delegate Event" ) );
		Assert.Contains( warning.Spans, s => s.Nav?.Kind == ErrorCheckTargetKind.Event && s.Nav.Name == "Relay" );
	}

	[Fact]
	public void CheckErrors_AcceptsPersistentEventWithTokenDelegate()
	{
		var scenario = CheckScenario();
		var tokenEvent = new TextInteraction( "Search Token" );
		tokenEvent.isTokenInteraction = true;
		scenario.interactionObserver.Add( tokenEvent );
		var persistent = new PersistentTokenInteraction( "Relay" );
		persistent.eventToActivate = "Search Token";
		scenario.interactionObserver.Add( persistent );

		var checker = RunCheck( scenario );

		Assert.DoesNotContain( "Delegate Event", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsMissingTokenTriggeredBy()
	{
		var scenario = CheckScenario();
		var tokenEvent = new TextInteraction( "Search Token" );
		tokenEvent.isTokenInteraction = true;
		scenario.interactionObserver.Add( tokenEvent );

		var chapter = new Chapter( "Start" );
		var tile = new HexTile( 200, skipBuild: true );
		tile.tokenList.Add( new Token( TokenType.Search )
		{
			triggerName = "Search Token",
			triggeredByName = "Ghost Trigger"
		} );
		chapter.tileObserver.Add( tile );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.Contains(
			"Token on tile 200 in tile block \"Start\" Triggered By points at missing trigger \"Ghost Trigger\".",
			checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsMissingChapterTriggers()
	{
		var scenario = CheckScenario();
		var chapter = new Chapter( "Later" );
		chapter.triggeredBy = "Reveal Later";
		chapter.exploreTrigger = "First Explore";
		chapter.exploredAllTilesTrigger = "All Explored";
		var tile = new HexTile( 200, skipBuild: true );
		tile.triggerName = "Tile Explored";
		chapter.tileObserver.Add( tile );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.Contains( "Tile block \"Later\" Triggered By points at missing trigger \"Reveal Later\".", checker.Errors );
		Assert.Contains( "Tile block \"Later\" Trigger On First Exploration points at missing trigger \"First Explore\".", checker.Errors );
		Assert.Contains( "Tile block \"Later\" Trigger After Exploring All Tiles points at missing trigger \"All Explored\".", checker.Errors );
		Assert.Contains( "Tile 200 in tile block \"Later\" Explored trigger points at missing trigger \"Tile Explored\".", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsReplaceTokenTargetThatIsNotTokenInteraction()
	{
		var scenario = CheckScenario();
		var flavorOnly = new TextInteraction( "Flavor Only" );
		flavorOnly.isTokenInteraction = false;
		scenario.interactionObserver.Add( flavorOnly );
		var replace = new ReplaceTokenInteraction( "Swap" );
		replace.eventToReplace = "Flavor Only";
		scenario.interactionObserver.Add( replace );

		var checker = RunCheck( scenario );

		Assert.Contains(
			"Event \"Swap\" Event To Replace points at event \"Flavor Only\" which is not a Token Interaction.",
			checker.Errors );
	}

	[Fact]
	public void CheckErrors_IgnoresTriggersThatExist()
	{
		var scenario = CheckScenario();
		scenario.triggersObserver.Add( new Trigger( "Exists" ) );
		var text = new TextInteraction( "Flavor" );
		text.triggerName = "Exists";
		text.triggerAfterName = "Exists";
		scenario.interactionObserver.Add( text );

		var checker = RunCheck( scenario );

		Assert.Equal( "None Found", checker.Errors );
	}

	[Fact]
	public void CheckErrors_LinksExistingEventOwnerButNotMissingTriggerName()
	{
		var scenario = CheckScenario();
		var text = new TextInteraction( "Flavor" );
		text.triggerName = "Missing Before";
		scenario.interactionObserver.Add( text );

		var checker = RunCheck( scenario );

		var warning = Assert.Single( checker.WarningItems );
		var owner = Assert.Single( warning.Spans, s => s.Nav?.Kind == ErrorCheckTargetKind.Event );
		Assert.Equal( "Flavor", owner.Nav!.Name );
		Assert.Contains( warning.Spans, s => s.Text.Contains( "Missing Before" ) && s.Nav == null );
	}

	[Fact]
	public void CheckErrors_ReportsComponentsFromDisabledCollections()
	{
		var scenario = CheckScenario();

		var items = new ItemInteraction( "Give Gear" );
		items.itemList.Add( Items.FromID( 26 )! ); // Shaded Paths Ring Mail
		items.itemList.Add( Items.FromID( 207 )! ); // Unfurling War Friendly Pony
		scenario.interactionObserver.Add( items );

		var titles = new TitleInteraction( "Give Title" );
		titles.titleList.Add( Titles.FromID( 25 )! ); // Shaded Paths Treasure Seeker
		scenario.interactionObserver.Add( titles );

		var threat = new ThreatInteraction( "Ambush" );
		threat.includedEnemies[10] = true; // Giant Spider (Shaded Paths)
		threat.monsterCollection.Add( new Monster( 10 ) );
		scenario.interactionObserver.Add( threat );

		var dgEvent = new TextInteraction( "Mud" );
		dgEvent.isTokenInteraction = true;
		dgEvent.tokenType = TokenType.DifficultGround;
		scenario.interactionObserver.Add( dgEvent );

		var chapter = new Chapter( "Start" );
		var tile = new HexTile( 213, skipBuild: true );
		tile.tokenList.Add( new Token( TokenType.Terrain ) { terrainType = TerrainType.Log } );
		tile.tokenList.Add( new Token( TokenType.Fortified ) );
		chapter.tileObserver.Add( tile );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.Contains( ErrorChecker.DisabledCollectionsHeading, checker.Errors );
		Assert.Contains( ErrorChecker.DisabledCollectionsSettingsLink, checker.Errors );
		Assert.Contains( "Event \"Give Gear\" uses Item \"Ring Mail\" from Shaded Paths.", checker.Errors );
		Assert.Contains( "Event \"Give Gear\" uses Mount \"Friendly Pony\" from Unfurling War.", checker.Errors );
		Assert.Contains( "Event \"Give Title\" uses Title \"Treasure Seeker\" from Shaded Paths.", checker.Errors );
		Assert.Contains( "uses Monster", checker.Errors );
		Assert.Contains( "from Shaded Paths.", checker.Errors );
		Assert.Contains( "Event \"Mud\" uses Difficult Ground, which needs Shaded Paths or Unfurling War.", checker.Errors );
		Assert.Contains( "Tile 213 in tile block \"Start\" is from Shaded Paths.", checker.Errors );
		Assert.Contains( "uses Terrain: Log from Shaded Paths.", checker.Errors );
		Assert.Contains( "uses Fortified, which needs Unfurling War.", checker.Errors );

		Assert.All(
			checker.WarningItems.Where( w => w.Section == ErrorCheckSection.DisabledCollections ),
			w => Assert.Equal( ErrorCheckSection.DisabledCollections, w.Section ) );
		Assert.Contains(
			checker.WarningItems,
			w => w.Spans.Any( s => s.Nav?.Kind == ErrorCheckTargetKind.TileEditor && s.Nav.Name == "Start" ) );
		Assert.Contains(
			checker.WarningItems,
			w => w.Spans.Any( s => s.Nav?.Kind == ErrorCheckTargetKind.Event && s.Nav.Name == "Give Gear" ) );
	}

	[Fact]
	public void CheckErrors_AllowsCollectionComponentsWhenCollectionsAreEnabled()
	{
		var scenario = CheckScenario();
		scenario.collectionObserver.Add( Collection.SHADED_PATHS );
		scenario.collectionObserver.Add( Collection.UNFURLING_WAR );

		var items = new ItemInteraction( "Give Gear" );
		items.itemList.Add( Items.FromID( 26 )! );
		items.itemList.Add( Items.FromID( 207 )! );
		scenario.interactionObserver.Add( items );

		var titles = new TitleInteraction( "Give Title" );
		titles.titleList.Add( Titles.FromID( 25 )! );
		scenario.interactionObserver.Add( titles );

		var threat = new ThreatInteraction( "Ambush" );
		threat.includedEnemies[10] = true;
		threat.monsterCollection.Add( new Monster( 10 ) );
		scenario.interactionObserver.Add( threat );

		var dgEvent = new TextInteraction( "Mud" );
		dgEvent.isTokenInteraction = true;
		dgEvent.tokenType = TokenType.DifficultGround;
		scenario.interactionObserver.Add( dgEvent );

		var chapter = new Chapter( "Start" );
		var tile = new HexTile( 213, skipBuild: true );
		tile.tokenList.Add( new Token( TokenType.Terrain ) { terrainType = TerrainType.Log } );
		tile.tokenList.Add( new Token( TokenType.Fortified ) );
		chapter.tileObserver.Add( tile );
		scenario.chapterObserver.Add( chapter );

		var checker = RunCheck( scenario );

		Assert.Equal( "None Found", checker.Errors );
		Assert.Equal( "Warnings (0):", checker.errorCount );
	}

	[Fact]
	public void CheckErrors_ReportsDefaultIntroductionObjectiveAndResolutionText()
	{
		var scenario = CheckScenario();
		scenario.introBookData = new TextBookData( "Default Introduction Text" );
		scenario.introBookData.pages.Add(
			"Default Introduction text.\n\nThis text is displayed at the beginning of the Scenario to describe the mission and Objectives.\n\nScenarios have one Introduction Text." );

		var success = new TextBookData( "Default Success Resolution" );
		success.pages.Add( "Default resolution text.  This text is displayed when the Scenario has ended in SUCCESS." );
		scenario.resolutionObserver.Add( success );
		var failure = new TextBookData( "Default Failure Resolution" );
		failure.pages.Add( "Default resolution text.  This text is displayed when the Scenario has ended in FAILURE." );
		scenario.resolutionObserver.Add( failure );
		var lastStand = new TextBookData( "Default Last Stand Failed Resolution" );
		lastStand.pages.Add( "You have succumbed to the terror and trauma of battle." );
		scenario.resolutionObserver.Add( lastStand );
		scenario.objectiveObserver.Add( new Objective( "Default Objective" ) );

		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );

		Assert.Contains( "Introduction Text still uses the default placeholder.", checker.Errors );
		Assert.Contains( "Resolution \"Default Success Resolution\" still uses the default Resolution Text.", checker.Errors );
		Assert.Contains( "Resolution \"Default Failure Resolution\" still uses the default Resolution Text.", checker.Errors );
		Assert.DoesNotContain( "Last Stand Failed", checker.Errors );
		Assert.Contains( "Objective \"Default Objective\" still uses the default Objective Reminder.", checker.Errors );
		Assert.Contains( "Objective \"Default Objective\" still uses the default Objective Summary.", checker.Errors );
		Assert.Contains(
			checker.WarningItems,
			w => w.Spans.Any( s => s.Nav?.Kind == ErrorCheckTargetKind.Settings ) );
	}

	[Fact]
	public void CheckErrors_SkipsObjectiveSummaryWhenSkipSummaryIsChecked()
	{
		SeedSnapGrids();
		var scenario = new Scenario( "Check" );
		UseCustomPlayerText( scenario );
		var objective = new Objective( "Side Quest" );
		objective.skipSummary = true;
		scenario.objectiveObserver.Add( objective );

		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );

		Assert.Contains( "Objective \"Side Quest\" still uses the default Objective Reminder.", checker.Errors );
		Assert.DoesNotContain( "Objective \"Side Quest\" still uses the default Objective Summary.", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsEventFlavorOnlyWhenTokenInteraction()
	{
		SeedSnapGrids();
		var scenario = new Scenario( "Check" );
		UseCustomPlayerText( scenario );
		var tokenEvent = new TextInteraction( "Search Token" );
		tokenEvent.isTokenInteraction = true;
		scenario.interactionObserver.Add( tokenEvent );
		var triggered = new TextInteraction( "Triggered Only" );
		triggered.isTokenInteraction = false;
		scenario.interactionObserver.Add( triggered );

		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );

		Assert.Contains( "Event \"Search Token\" still uses the default Flavor Text.", checker.Errors );
		Assert.Contains( "Event \"Search Token\" still uses the default Event Text.", checker.Errors );
		Assert.DoesNotContain( "Event \"Triggered Only\" still uses the default Flavor Text.", checker.Errors );
		Assert.Contains( "Event \"Triggered Only\" still uses the default Event Text.", checker.Errors );
	}

	[Fact]
	public void CheckErrors_SkipsDefaultTextForConditionalPersistentAndSilentReplace()
	{
		SeedSnapGrids();
		var scenario = new Scenario( "Check" );
		UseCustomPlayerText( scenario );
		scenario.interactionObserver.Add( new ConditionalInteraction( "Listen" ) );
		scenario.interactionObserver.Add( new PersistentTokenInteraction( "Relay" ) );
		var replace = new ReplaceTokenInteraction( "Swap" );
		replace.noText = true;
		scenario.interactionObserver.Add( replace );

		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );

		Assert.DoesNotContain( "Event \"Listen\" still uses the default", checker.Errors );
		Assert.DoesNotContain( "Event \"Relay\" still uses the default Flavor Text.", checker.Errors );
		Assert.DoesNotContain( "Event \"Relay\" still uses the default Event Text.", checker.Errors );
		Assert.Contains( "Event \"Relay\" still uses the default Alternative Flavor Text.", checker.Errors );
		Assert.DoesNotContain( "Event \"Swap\" still uses the default", checker.Errors );
	}

	[Fact]
	public void CheckErrors_ReportsDefaultCustomAttackGroupText()
	{
		SeedSnapGrids();
		var scenario = new Scenario( "Check" );
		UseCustomPlayerText( scenario );
		var group = new MonsterActivations( MonsterActivations.START_OF_CUSTOM_ACTIVATIONS );
		group.dataName = "Custom Wolves";
		group.activations.Add( new MonsterActivationItem( 1 ) );
		scenario.activationsObserver.Add( group );

		var checker = new ErrorChecker();
		checker.CheckErrors( scenario );

		Assert.Contains( "Enemy Attack Group \"Custom Wolves\" still uses the default Attack Text.", checker.Errors );
		Assert.Contains( "Enemy Attack Group \"Custom Wolves\" still uses the default After Effect.", checker.Errors );
		Assert.Contains(
			checker.WarningItems,
			w => w.Spans.Any( s => s.Nav?.Kind == ErrorCheckTargetKind.Activation && s.Nav.Name == "Custom Wolves" ) );
	}
}
