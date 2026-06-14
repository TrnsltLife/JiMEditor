using System;
using System.Collections.ObjectModel;
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
		var seed = _random.Next();
		var outcome = await Task.Run( () => EnemyEncounterSimulator.Simulate( included, scaledPool, seed ) )
			.ConfigureAwait( false );

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
}
