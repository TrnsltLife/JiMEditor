using System;
using System.Threading;
using Avalonia;
using JiMEditor.Avalonia;
using JiME;
using Xunit.Sdk;

namespace JiMEditor.Avalonia.Tests;

/// <summary>Shared STA thread + minimal <see cref="Utils"/> seeding for model-layer tests.</summary>
internal static class ModelTestEnvironment
{
	static readonly object AvaloniaInitLock = new();
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
		thread.SetApartmentState( ApartmentState.STA );
		thread.Start();
		thread.Join();
		if ( threadException != null )
			throw new XunitException( threadException.ToString() );
	}

	internal static void SetupMinimalUtilsForModelTests()
	{
		EnsureAvaloniaInitialized();
		Utils.dragSnapX = [0d, 48d, 96d, 144d, 192d, 240d];
		Utils.dragSnapY = [0d, 27.7128128d, 55.4256256d, 83.1384384d, 110.8512512d, 138.564064d];
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
