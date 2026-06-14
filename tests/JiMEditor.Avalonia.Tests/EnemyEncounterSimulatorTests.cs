using System.Linq;
using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class EnemyEncounterSimulatorTests
{
	[Fact]
	public void Simulate_respects_per_type_figure_pool_limits()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();
			Utils.Init();

			var included = new bool[30];
			included[(int)MonsterType.HillTroll] = true;
			var outcome = EnemyEncounterSimulator.Simulate( included, 500f, seed: 42 );
			int total = outcome.Generated
				.Where( g => g.monsterType == MonsterType.HillTroll )
				.Sum( g => g.count );
			Assert.InRange( total, 0, 1 );
		} );
	}

	[Fact]
	public void Simulate_never_exceeds_goblin_scout_figure_supply()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();
			Utils.Init();

			var included = new bool[30];
			included[(int)MonsterType.GoblinScout] = true;
			var outcome = EnemyEncounterSimulator.Simulate( included, 250f, seed: 991 );
			int total = outcome.Generated
				.Where( g => g.monsterType == MonsterType.GoblinScout )
				.Sum( g => g.count );
			Assert.InRange( total, 0, 6 );
		} );
	}

	[Fact]
	public void Simulate_modifier_names_come_from_basic_modifier_list()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();
			Utils.Init();

			var included = new bool[30];
			included[(int)MonsterType.Ruffian] = true;
			var basicNames = MonsterModifier.BasicValues.Select( m => m.name ).ToHashSet();
			for ( int seed = 0; seed < 200; seed++ )
			{
				var outcome = EnemyEncounterSimulator.Simulate( included, 80f, seed );
				foreach ( var group in outcome.Generated )
				{
					Assert.True( group.modList.Count <= EnemyEncounterSimulator.MaxModifiers );
					foreach ( var modName in group.modList )
						Assert.Contains( modName, basicNames );
				}
			}
		} );
	}
}
