using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class EnemyCalculatorWindow : Window
{
	readonly SimulatorData _simulatorData;
	readonly Random _random = new();
	readonly int[] _monsterCost = Collection.CORE_SET.Monsters.Select( m => m.cost[0] )
		.Concat( Collection.VILLAINS_OF_ERIAJAR.Monsters.Select( m => m.cost[0] ) )
		.Concat( Collection.SHADED_PATHS.Monsters.Select( m => m.cost[0] ) )
		.Concat( Collection.DENIZENS_IN_DARKNESS.Monsters.Select( m => m.cost[0] ) )
		.Concat( Collection.UNFURLING_WAR.Monsters.Select( m => m.cost[0] ) )
		.Concat( Collection.SCORCHERS_OF_THE_WILDS.Monsters.Select( m => m.cost[0] ) ).ToArray();
	readonly string[] _modNames = { "Large", "Bloodthirsty", "Armored", "Huge", "Shrouded", "Terrifying", "Spike Armor", "Well-Equipped", "Veteran", "Wary", "Guarded", "Hardened", "Alert" };
	readonly int[] _modCost = { 3, 6, 4, 4, 7, 3, 6, 6, 10, 8, 9, 6, 10 };
	bool _simulationReady;
	int _rerollSeq;

	public ObservableCollection<MonsterSim> CalculatedItems { get; } = new();
	public EnemyCalculatorWindow()
	{
		InitializeComponent();
		_simulatorData = new SimulatorData { poolPoints = 10, difficultyBias = DifficultyBias.Medium };
		ResultsItems.ItemsSource = CalculatedItems;
		HookLoadedForInitialSimulation();
	}

	public EnemyCalculatorWindow( SimulatorData simData )
	{
		InitializeComponent();
		_simulatorData = simData;
		ResultsItems.ItemsSource = CalculatedItems;
		BasePointsText.Text = $"Base Pool Points: {(int)simData.poolPoints}";
		BiasText.Text = $"Difficulty Bias: {simData.difficultyBias}";
		HookLoadedForInitialSimulation();
	}

	void HookLoadedForInitialSimulation()
	{
		Opened += OnFirstOpened;
	}

	void OnFirstOpened( object? sender, EventArgs e )
	{
		Opened -= OnFirstOpened;
		_simulationReady = true;
		RerollAsync();
	}


	void OkButton_OnClick( object? sender, RoutedEventArgs e ) => Close();

	async void PlayersCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _simulationReady )
			await RerollAsync();
	}

	async void DifficultyCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _simulationReady )
			await RerollAsync();
	}

	async void RerollButton_OnClick( object? sender, RoutedEventArgs e ) => await RerollAsync();

	void SetSimulatorWarning( string? message )
	{
		if ( SimulatorWarningText == null )
			return;
		if ( string.IsNullOrEmpty( message ) )
		{
			SimulatorWarningText.IsVisible = false;
			SimulatorWarningText.Text = "";
		}
		else
		{
			SimulatorWarningText.Text = message;
			SimulatorWarningText.IsVisible = true;
		}
	}

	async Task RerollAsync()
	{
		if ( !_simulationReady )
			return;
		var seq = ++_rerollSeq;
		SetSimulatorWarning( null );
		CalculatedItems.Clear();

		var playersIndex = PlayersCombo.SelectedIndex;
		var difficultyIndex = DifficultyCombo.SelectedIndex;
		var scaledPool = CalculateScaledPointsValue( playersIndex, difficultyIndex );
		ScaledPointsText.Text = scaledPool.ToString();

		var included = (bool[])_simulatorData.includedEnemies.Clone();
		var slotCount = System.Math.Min( included.Length, _monsterCost.Length );
		var lowestCost = LowestCostFromIncluded( included, slotCount, _monsterCost );
		if ( lowestCost == 1000 )
		{
			SetSimulatorWarning( "There are no Enemies included in the Pool." );
			ModPointsText.Text = "0";
			LeftOversText.Text = "0";
			return;
		}

		if ( scaledPool < lowestCost )
		{
			SetSimulatorWarning( "There aren't enough Pool Points to generate any Enemies given the current parameters." );
			ModPointsText.Text = "0";
			LeftOversText.Text = "0";
			return;
		}

		var seed = _random.Next();
		var outcome = await Task.Run( () => EnemySimulatorCore.Simulate(
			included,
			slotCount,
			_monsterCost,
			_modNames,
			_modCost,
			scaledPool,
			lowestCost,
			seed ) ).ConfigureAwait( false );

		await Dispatcher.UIThread.InvokeAsync( () =>
		{
			if ( seq != _rerollSeq )
				return;
			if ( !string.IsNullOrEmpty( outcome.Warning ) )
				SetSimulatorWarning( outcome.Warning );
			foreach ( var sim in outcome.Generated )
				CalculatedItems.Add( sim );
			ModPointsText.Text = outcome.ModPoints.ToString();
			LeftOversText.Text = outcome.LeftOvers.ToString();
		} );
	}

	static float CalculateScaledPointsValue( int difficultyIndex, int playersIndex, float poolPoints, DifficultyBias difficultyBias )
	{
		float difficultyScale = difficultyIndex == 0 ? -.25f : difficultyIndex == 2 ? .5f : 0f;
		int bias = difficultyBias == DifficultyBias.Light ? 3 : difficultyBias == DifficultyBias.Heavy ? 7 : 5;
		float pool = poolPoints;
		pool += playersIndex * bias;
		pool += pool * difficultyScale;
		return pool;
	}

	float CalculateScaledPointsValue( int playersIndex, int difficultyIndex ) =>
		CalculateScaledPointsValue( difficultyIndex, playersIndex, _simulatorData.poolPoints, _simulatorData.difficultyBias );

	static int LowestCostFromIncluded( bool[] included, int slotCount, int[] monsterCost )
	{
		int cost = 1000;
		for ( int i = 0; i < slotCount; i++ )
			if ( included[i] && monsterCost[i] < cost )
				cost = monsterCost[i];
		return cost;
	}

	static class EnemySimulatorCore
	{
		public sealed class SimOutcome
		{
			public List<MonsterSim> Generated { get; } = new();
			public string? Warning { get; set; }
			public int ModPoints { get; set; }
			public float LeftOvers { get; set; }
		}

		public static SimOutcome Simulate(
			bool[] includedEnemies,
			int slotCount,
			int[] monsterCost,
			string[] modNames,
			int[] modCost,
			float scaledPool,
			int lowestCost,
			int seed )
		{
			var outcome = new SimOutcome();
			var random = new Random( seed );
			int includedCount = 0;
			for ( int i = 0; i < slotCount; i++ )
				if ( includedEnemies[i] )
					includedCount++;

			float poolCount = scaledPool;
			float starting = poolCount;
			int numSingleGroups = 0;
			int lastEnemyIndex = -1;
			int modPointsPhase = 0;

			var generated = outcome.Generated;
			const int maxGenerationSteps = 100_000;
			var step = 0;
			while ( poolCount >= lowestCost && step < maxGenerationSteps )
			{
				step++;
				var poolBefore = poolCount;
				var t = GenerateMonster( includedEnemies, slotCount, monsterCost, includedCount, ref numSingleGroups, ref lastEnemyIndex, poolCount, random, modPointsPhase );
				if ( t.Item1.dataName != "modifier" )
				{
					var spend = t.Item2;
					if ( spend <= 0f )
						spend = t.Item1.cost > 0f ? t.Item1.cost : lowestCost;
					poolCount = Math.Max( 0f, poolCount - spend );
					generated.Add( t.Item1 );
				}
				else
					poolCount = Math.Max( 0f, poolCount - 1f );
				if ( poolCount >= poolBefore )
					break;
			}

			if ( poolCount >= lowestCost )
				outcome.Warning = "Simulation could not finish (safety limit or stalled pool). Try other pool settings or selections.";

			poolCount = starting;
			foreach ( var sim in generated )
				poolCount -= sim.cost;

			if ( poolCount > lowestCost )
			{
				foreach ( var sim in generated )
				{
					if ( random.Next( 100 ) < 50 && sim.count < 2 && sim.singlecost <= poolCount )
					{
						poolCount = Math.Max( 0f, poolCount - sim.singlecost );
						sim.count++;
						sim.cost += sim.singlecost;
					}
				}
			}

			if ( generated.Count > 0 && poolCount > 0 )
			{
				foreach ( var sim in generated )
				{
					int mod = random.Next( 4 );
					if ( mod != 3 && modCost[mod] <= poolCount )
					{
						poolCount -= modCost[mod];
						sim.modList.Add( modNames[mod] );
						outcome.ModPoints += modCost[mod];
					}
				}
			}

			outcome.LeftOvers = (float)Math.Round( poolCount * 100f ) / 100f;
			return outcome;
		}

		static Tuple<MonsterSim, float> GenerateMonster(
			bool[] includedEnemies,
			int slotCount,
			int[] monsterCost,
			int includedCount,
			ref int numSingleGroups,
			ref int lastEnemyIndex,
			float points,
			Random random,
			int modPoints )
		{
			List<Tuple<MonsterType, float>> list = new();
			for ( int i = 0; i < slotCount; i++ )
			{
				if ( includedCount > 1 && lastEnemyIndex == i )
					continue;
				if ( includedEnemies[i] && points >= monsterCost[i] )
					list.Add( new Tuple<MonsterType, float>( (MonsterType)i, monsterCost[i] ) );
			}

			if ( list.Count == 0 )
				return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 1 }, 1 );

			int pick = random.Next( 0, list.Count );
			MonsterSim ms = new() { dataName = list[pick].Item1.ToString() };
			float groupCost = list[pick].Item2;
			int upTo = groupCost <= points ? 1 : 0;
			for ( int i = 0; i < 2; i++ )
				if ( ( groupCost + .6f * list[pick].Item2 ) <= points )
				{
					upTo++;
					groupCost += .6f * list[pick].Item2;
				}
			int count = random.Next( 1, upTo + 1 );
			if ( count == 1 && numSingleGroups >= 1 )
			{
				if ( count + 1 <= upTo )
				{
					if ( random.Next( 100 ) > 50 || modPoints > 3 )
						count += 1;
					else
						return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 1 }, 1 );
				}
				else
					return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 0 }, random.Next( 100 ) > 30 ? 1 : 0 );
			}

			if ( count == 1 )
				numSingleGroups++;
			groupCost = list[pick].Item2 + ( ( count - 1 ) * ( .6f * list[pick].Item2 ) );
			lastEnemyIndex = (int)list[pick].Item1;
			ms.count = count;
			ms.singlecost = list[pick].Item2;
			ms.cost = groupCost;
			return new Tuple<MonsterSim, float>( ms, list[pick].Item2 * count );
		}
	}
}
