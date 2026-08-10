using System;
using System.Threading;
using Avalonia;
using JiMEditor.Avalonia;
using JiME;
using Xunit;
using Xunit.Sdk;

// Utils / Avalonia app state is process-wide; parallel classes were racing SetupMinimalUtils
// (clearing defaultStats) against EnemyEncounterSimulator → Monster fallback figureLimit=6.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace JiMEditor.Avalonia.Tests;

/// <summary>Shared STA thread + minimal <see cref="Utils"/> seeding for model-layer tests.</summary>
internal static class ModelTestEnvironment
{
	static readonly object AvaloniaInitLock = new();
	static readonly object UtilsLock = new();
	static bool AvaloniaInitialized;

	internal static void RunInSta( Action action )
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
		// STA is Windows-only; on Mac/Linux Avalonia tests still run on a dedicated thread.
		if ( OperatingSystem.IsWindows() )
			thread.SetApartmentState( ApartmentState.STA );
		thread.Start();
		thread.Join();
		if ( threadException != null )
			throw new XunitException( threadException.ToString() );
	}

	/// <summary>Seed snap grids; do not wipe already-loaded enemy defaults (parallel-safe under assembly lock).</summary>
	internal static void SetupMinimalUtilsForModelTests()
	{
		lock ( UtilsLock )
		{
			EnsureAvaloniaInitialized();
			if ( Utils.dragSnapX == null || Utils.dragSnapX.Length < 6 )
			{
				Utils.dragSnapX = [0d, 48d, 96d, 144d, 192d, 240d];
				Utils.dragSnapY = [0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d];
			}

			Utils.defaultStats ??= [];
			Utils.defaultActivations ??= [];
			Utils.defaultTerrainInteractions ??= [];
		}
	}

	/// <summary>Ensure snap grids + <see cref="Utils.Init"/> enemy/tile data are loaded (one-shot under lock).</summary>
	internal static void EnsureUtilsInitialized()
	{
		lock ( UtilsLock )
		{
			EnsureAvaloniaInitialized();
			if ( Utils.dragSnapX == null || Utils.dragSnapX.Length < 6 )
			{
				Utils.dragSnapX = [0d, 48d, 96d, 144d, 192d, 240d];
				Utils.dragSnapY = [0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d];
			}

			if ( Utils.defaultStats == null || Utils.defaultStats.Length == 0 )
				Utils.Init();
		}
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
