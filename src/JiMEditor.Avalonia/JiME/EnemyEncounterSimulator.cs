// Ported from: your-journey/YourJourney/Assets/Scripts/Engine/Interactions/EnemyInteraction.cs (ThreatInteraction.GenerateEncounter)
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiME;

/// <summary>
/// Simulates random enemy pool encounters using the same rules as the Companion App.
/// </summary>
public static class EnemyEncounterSimulator
{
	public const int MaxModifiers = 7;
	public const int ReasonableModifiers = 3;

	public sealed class SimOutcome
	{
		public List<MonsterSim> Generated { get; } = new();
		public string? Warning { get; set; }
		public int ModPoints { get; set; }
		public float LeftOvers { get; set; }
	}

	sealed class EncounterGroup
	{
		public MonsterSim Sim { get; }
		public HashSet<int> ModifierIds { get; } = new();

		public EncounterGroup( MonsterSim sim ) => Sim = sim;

		public bool TryAddModifier( MonsterModifier mod )
		{
			if ( ModifierIds.Count >= MaxModifiers || ModifierIds.Contains( mod.id ) )
				return false;
			ModifierIds.Add( mod.id );
			Sim.modList.Add( mod.name );
			return true;
		}
	}

	sealed class MonsterArrays
	{
		public int[] Cost { get; }
		public int[] FigureLimit { get; }
		public int[] GroupLimit { get; }

		public MonsterArrays( int count )
		{
			Cost = new int[count];
			FigureLimit = new int[count];
			GroupLimit = new int[count];
			for ( int i = 0; i < count; i++ )
			{
				var monster = new Monster( i );
				Cost[i] = monster.cost[0];
				FigureLimit[i] = monster.figureLimit;
				GroupLimit[i] = monster.groupLimit;
			}
		}
	}

	static readonly int s_monsterSlotCount = Enum.GetValues<MonsterType>().Length;

	public static SimOutcome Simulate( bool[] includedEnemies, float scaledPool, int seed )
	{
		var stats = new MonsterArrays( s_monsterSlotCount );
		var outcome = new SimOutcome();
		var random = new Random( seed );
		var slotCount = Math.Min( includedEnemies.Length, stats.Cost.Length );
		var lowestCost = LowestRequestedEnemyCost( includedEnemies, slotCount, stats.Cost );
		if ( lowestCost == 1000 )
		{
			outcome.Warning = "There are no Enemies included in the Pool.";
			return outcome;
		}

		if ( scaledPool < lowestCost )
		{
			outcome.Warning = "There aren't enough Pool Points to generate any Enemies given the current parameters.";
			return outcome;
		}

		var monsterPool = stats.FigureLimit.ToArray();
		int includedCount = 0;
		for ( int i = 0; i < slotCount; i++ )
			if ( includedEnemies[i] )
				includedCount++;

		float poolCount = scaledPool;
		float starting = poolCount;
		int numSingleGroups = 0;
		int lastEnemyIndex = -1;
		int modPoints = 0;
		var groups = new List<EncounterGroup>();

		float lastPoolCount = -1000f;
		const int maxGenerationSteps = 100_000;
		var step = 0;
		while ( poolCount >= lowestCost && poolCount != lastPoolCount && step < maxGenerationSteps )
		{
			step++;
			lastPoolCount = poolCount;
			var generated = GenerateMonster(
				includedEnemies,
				slotCount,
				includedCount,
				stats,
				monsterPool,
				ref numSingleGroups,
				ref lastEnemyIndex,
				poolCount,
				modPoints,
				random );
			if ( generated.Item1.dataName != "modifier" )
			{
				poolCount = Math.Max( 0f, poolCount - generated.Item2 );
				groups.Add( new EncounterGroup( generated.Item1 ) );
			}
			else
				poolCount = Math.Max( 0f, poolCount - 1f );
		}

		if ( step >= maxGenerationSteps )
			outcome.Warning = "Simulation could not finish (safety limit). Try other pool settings or selections.";

		poolCount = starting;
		foreach ( var group in groups )
			poolCount -= group.Sim.cost;

		if ( poolCount > lowestCost )
		{
			foreach ( var group in groups )
			{
				var sim = group.Sim;
				if ( random.Next( 100 ) < 50
				     && sim.count < 2
				     && sim.count < sim.groupLimit
				     && LeftInPool( monsterPool, sim.monsterType ) > 0
				     && sim.singlecost <= poolCount )
				{
					poolCount = Math.Max( 0f, poolCount - sim.singlecost );
					sim.count++;
					RemoveFromPool( monsterPool, sim.monsterType, 1 );
					sim.cost += sim.singlecost;
				}
			}
		}

		bool foundModifiers = true;
		while ( foundModifiers && poolCount > 1 )
		{
			foundModifiers = false;
			if ( groups.Count == 0 || poolCount <= 0 )
				break;

			foreach ( var group in groups )
			{
				var sim = group.Sim;
				if ( group.ModifierIds.Count >= MaxModifiers || group.ModifierIds.Count >= ReasonableModifiers )
					continue;

				var modList = MonsterModifier.ListAvailableModifiersFor( sim.monsterType, sim.count, (int)poolCount );
				int modIndex = random.Next( modList.Count + 1 );
				if ( modIndex == modList.Count )
					continue;

				var mod = modList[modIndex];
				if ( !group.TryAddModifier( mod ) )
					continue;

				int modCost = mod.CalculateCost( sim.count );
				poolCount -= modCost;
				modPoints += modCost;
				foundModifiers = true;
			}
		}

		outcome.ModPoints = modPoints;
		outcome.LeftOvers = (float)Math.Round( poolCount * 100f ) / 100f;
		foreach ( var group in groups )
			outcome.Generated.Add( group.Sim );
		return outcome;
	}

	static int LowestRequestedEnemyCost( bool[] includedEnemies, int slotCount, int[] monsterCost )
	{
		int cost = 1000;
		for ( int i = 0; i < slotCount; i++ )
		{
			if ( includedEnemies[i] && monsterCost[i] < cost )
				cost = monsterCost[i];
		}
		return cost;
	}

	static Tuple<MonsterSim, float> GenerateMonster(
		bool[] includedEnemies,
		int slotCount,
		int includedCount,
		MonsterArrays stats,
		int[] monsterPool,
		ref int numSingleGroups,
		ref int lastEnemyIndex,
		float points,
		int modPoints,
		Random random )
	{
		var candidates = new List<Tuple<MonsterType, float>>();
		for ( int i = 0; i < slotCount; i++ )
		{
			if ( includedCount > 1 && lastEnemyIndex == i )
				continue;
			if ( LeftInPool( monsterPool, (MonsterType)i ) <= 0 )
				continue;
			if ( includedEnemies[i] && points >= stats.Cost[i] )
				candidates.Add( new Tuple<MonsterType, float>( (MonsterType)i, stats.Cost[i] ) );
		}

		if ( candidates.Count == 0 )
			return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 1 }, 1 );

		int pick = random.Next( 0, candidates.Count );
		var monsterType = candidates[pick].Item1;
		var ms = CreateMonsterSim( monsterType, stats );
		float groupCost = candidates[pick].Item2;
		int upTo = groupCost <= points ? 1 : 0;
		int leftInPool = LeftInPool( monsterPool, monsterType );
		for ( int i = 0; i < 2 && i < leftInPool; i++ )
		{
			if ( ( groupCost + .6f * candidates[pick].Item2 ) <= points )
			{
				upTo += 1;
				groupCost += .6f * candidates[pick].Item2;
			}
		}

		int count = random.Next( 1, upTo + 1 );
		if ( count == 1 && numSingleGroups >= 1 )
		{
			if ( count + 1 <= upTo && count + 1 <= leftInPool )
			{
				if ( random.Next( 100 ) > 50 || modPoints > 3 )
					count += 1;
				else
					return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 1 }, 0 );
			}
			else
				return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 0 }, random.Next( 100 ) > 30 ? 1 : 0 );
		}

		if ( count > leftInPool )
			count = leftInPool;
		if ( count > ms.groupLimit )
			count = ms.groupLimit;
		groupCost = candidates[pick].Item2 + ( ( count - 1 ) * ( .6f * candidates[pick].Item2 ) );
		lastEnemyIndex = (int)monsterType;
		ms.count = count;
		if ( count > 0 )
			RemoveFromPool( monsterPool, monsterType, count );
		ms.singlecost = candidates[pick].Item2;
		ms.cost = groupCost;
		if ( count == 1 )
			numSingleGroups++;
		else if ( count == 0 )
			return new Tuple<MonsterSim, float>( new MonsterSim { dataName = "modifier", cost = 1 }, 1 );

		return new Tuple<MonsterSim, float>( ms, candidates[pick].Item2 * count );
	}

	static MonsterSim CreateMonsterSim( MonsterType monsterType, MonsterArrays stats )
	{
		var template = new Monster( (int)monsterType );
		return new MonsterSim
		{
			monsterType = monsterType,
			dataName = template.dataName,
			health = template.health,
			shieldValue = template.shieldValue,
			sorceryValue = template.sorceryValue,
			groupLimit = stats.GroupLimit[(int)monsterType],
		};
	}

	static int LeftInPool( int[] monsterPool, MonsterType monsterType ) =>
		monsterPool[(int)monsterType];

	static void RemoveFromPool( int[] monsterPool, MonsterType monsterType, int amount )
	{
		if ( amount < 0 )
			return;
		monsterPool[(int)monsterType] -= amount;
	}
}
