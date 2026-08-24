using JiME;
using JiMEditor.Avalonia;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class ScenarioDirtyTrackingTests
{
	static void Arrange()
	{
		Utils.defaultStats ??= [];
		Utils.defaultActivations ??= [];
		Utils.defaultTerrainInteractions ??= [];
		if ( Utils.dragSnapX == null || Utils.dragSnapX.Length < 6 )
		{
			Utils.dragSnapX = [ 0d, 48d, 96d, 144d, 192d, 240d ];
			Utils.dragSnapY = [ 0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d ];
		}
	}

	[Fact]
	public void MarkDirty_sets_isDirty_and_is_idempotent()
	{
		Arrange();
		var scenario = new Scenario();
		scenario.TriggerTitleChange( false );
		Assert.False( scenario.isDirty );

		scenario.MarkDirty();
		Assert.True( scenario.isDirty );
		var token = scenario.titleChangedToken;

		scenario.MarkDirty();
		Assert.True( scenario.isDirty );
		Assert.Equal( token, scenario.titleChangedToken );
	}

	[Fact]
	public void PauseDirtyTracking_blocks_MarkDirty()
	{
		Arrange();
		var scenario = new Scenario();
		scenario.TriggerTitleChange( false );

		using ( scenario.PauseDirtyTracking() )
			scenario.MarkDirty();
		Assert.False( scenario.isDirty );

		scenario.MarkDirty();
		Assert.True( scenario.isDirty );
	}

	[Fact]
	public void Collection_tracker_marks_dirty_on_add_and_nested_tile_token()
	{
		Arrange();
		var scenario = new Scenario();
		scenario.TriggerTitleChange( false );

		using ( ScenarioDirty.Attach( scenario ) )
		{
			scenario.AddObjective( new Objective( "Dirty Test Objective" ) );
			Assert.True( scenario.isDirty );

			scenario.TriggerTitleChange( false );
			var tile = new HexTile( 101, skipBuild: true );
			scenario.chapterObserver[0].AddTile( tile );
			Assert.True( scenario.isDirty );

			scenario.TriggerTitleChange( false );
			tile.tokenList.Add( new Token( TokenType.Search ) );
			Assert.True( scenario.isDirty );

			scenario.TriggerTitleChange( false );
			scenario.filteredGlobalTilePool.Add( 202 );
			Assert.False( scenario.isDirty );
		}
	}
}
