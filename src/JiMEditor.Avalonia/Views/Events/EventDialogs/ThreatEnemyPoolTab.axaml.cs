using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class ThreatEnemyPoolTab : UserControl
{
	Scenario? _scenario;
	ThreatInteraction? _interaction;
	bool _suppress;
	bool _suppressCollectionUi;
	List<CheckBox> _all = new();
	List<CheckBox> _core = new();
	List<CheckBox> _villains = new();
	List<CheckBox> _shaded = new();
	List<CheckBox> _denizens = new();
	List<CheckBox> _unfurling = new();
	List<CheckBox> _scorchers = new();

	public event Action<string>? DefeatedTriggerChanged;

	public ThreatEnemyPoolTab()
	{
		InitializeComponent();
		WireLists();
	}

	public void Attach( Scenario scenario, ThreatInteraction interaction )
	{
		_scenario = scenario;
		_interaction = interaction;
		DefeatedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
	}

	public void Hydrate()
	{
		if ( _interaction == null || _scenario == null )
			return;
		BasePoolPointsTextBox.Text = _interaction.basePoolPoints.ToString( CultureInfo.InvariantCulture );
		BiasLightRadio.IsChecked = _interaction.difficultyBias == DifficultyBias.Light;
		BiasMediumRadio.IsChecked = _interaction.difficultyBias == DifficultyBias.Medium;
		BiasHeavyRadio.IsChecked = _interaction.difficultyBias == DifficultyBias.Heavy;
		for ( int i = 0; i < _all.Count && i < _interaction.includedEnemies.Length; i++ )
			_all[i].IsChecked = _interaction.includedEnemies[i];
		SetDefeatedTrigger( _interaction.triggerDefeatedName );
	}

	internal IReadOnlyList<CheckBox> CoreMonsterCheckBoxes => _core;
	internal IReadOnlyList<CheckBox> VillainsMonsterCheckBoxes => _villains;
	internal IReadOnlyList<CheckBox> ShadedMonsterCheckBoxes => _shaded;
	internal IReadOnlyList<CheckBox> DenizensMonsterCheckBoxes => _denizens;
	internal IReadOnlyList<CheckBox> UnfurlingMonsterCheckBoxes => _unfurling;
	internal IReadOnlyList<CheckBox> ScorchersMonsterCheckBoxes => _scorchers;

	internal void BeginSuppressCollectionUi() => _suppressCollectionUi = true;
	internal void EndSuppressCollectionUi() => _suppressCollectionUi = false;

	public void Commit()
	{
		if ( _interaction == null )
			return;
		if ( int.TryParse( BasePoolPointsTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pool ) )
			_interaction.basePoolPoints = pool;
		_interaction.difficultyBias = BiasLightRadio.IsChecked == true ? DifficultyBias.Light
			: BiasHeavyRadio.IsChecked == true ? DifficultyBias.Heavy : DifficultyBias.Medium;
		for ( int i = 0; i < _all.Count && i < _interaction.includedEnemies.Length; i++ )
			_interaction.includedEnemies[i] = _all[i].IsChecked == true;
	}

	public void SetDefeatedTrigger( string? triggerName )
	{
		if ( DefeatedTriggerCombo.ItemsSource is not IEnumerable src )
			return;
		_suppress = true;
		try
		{
			var list = src.Cast<Trigger>().ToList();
			DefeatedTriggerCombo.SelectedItem = list.FirstOrDefault( t => t.dataName == triggerName ) ?? list.FirstOrDefault( t => t.dataName == "None" );
		}
		finally { _suppress = false; }
	}

	/// <summary>Matches WPF ThreatInteractionWindow: enable only enemies whose collection is selected on the scenario.</summary>
	public void ApplyScenarioCollectionAvailability()
	{
		if ( _scenario == null )
			return;
		ScenarioCollectionAvailability.ApplyThreatEnemyPool( _scenario, this );
	}

	void WireLists()
	{
		_core = new() { RuffianCheckBox, GoblinScoutCheckBox, OrcHunterCheckBox, OrcMarauderCheckBox, HungryVargCheckBox, HillTrollCheckBox, WightCheckBox };
		_villains = new() { AtariCheckBox, GargletargCheckBox, ChartoothCheckBox };
		_shaded = new() { GiantSpiderCheckBox, PitGoblinCheckBox, OrcTaskmasterCheckBox, ShadowmanCheckBox, AnonymousThingCheckBox, CaveTrollCheckBox, BalerockCheckBox, SpawnOfUglygiantCheckBox };
		_denizens = new() { SupplicantCheckBox, UrsulaCheckBox, OliverCheckBox };
		_unfurling = new() { FoulBeastCheckBox, VargRiderCheckBox, SiegeEngineCheckBox, WarElephantCheckBox, SoldierCheckBox, HighOrcWarriorCheckBox };
		_scorchers = new() { LordJavelinCheckBox, LichKingCheckBox, EndrisCheckBox };
		_all = new();
		_all.AddRange( _core );
		_all.AddRange( _villains );
		_all.AddRange( _shaded );
		_all.AddRange( _denizens );
		_all.AddRange( _unfurling );
		_all.AddRange( _scorchers );
	}

	void CollectionCheckBox_OnChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppressCollectionUi )
			return;
		if ( sender is not CheckBox cb )
			return;
		List<CheckBox>? target = cb == CoreSetCheckBox ? _core
			: cb == VillainsCheckBox ? _villains
			: cb == ShadedPathsCheckBox ? _shaded
			: cb == DenizensCheckBox ? _denizens
			: cb == UnfurlingWarCheckBox ? _unfurling
			: cb == ScorchersCheckBox ? _scorchers
			: null;
		if ( target == null )
			return;
		var isChecked = cb.IsChecked == true;
		foreach ( var it in target )
			it.IsChecked = isChecked;
	}

	void DefeatedTriggerCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppress || _interaction == null )
			return;
		var name = DefeatedTriggerCombo.SelectedItem is Trigger t ? t.dataName : "None";
		_interaction.triggerDefeatedName = name;
		DefeatedTriggerChanged?.Invoke( name );
	}

	async void AddDefeatedTriggerButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _scenario == null || _interaction == null )
			return;
		if ( TopLevel.GetTopLevel( this ) is not Window owner )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;
		DefeatedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver ).ToList();
		SetDefeatedTrigger( tw.EnteredTriggerName );
		_interaction.triggerDefeatedName = tw.EnteredTriggerName;
		DefeatedTriggerChanged?.Invoke( tw.EnteredTriggerName );
	}

	async void SimulateButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction == null )
			return;
		Commit();
		var sd = new SimulatorData
		{
			poolPoints = _interaction.basePoolPoints,
			difficultyBias = _interaction.difficultyBias
		};
		for ( int i = 0; i < _all.Count && i < sd.includedEnemies.Length; i++ )
			sd.includedEnemies[i] = _all[i].IsChecked == true;
		await Task.Yield();
		var sim = new EnemyCalculatorWindow( sd );
		await sim.ShowDialog( OwnerWindowForDialog( this ) );
	}

	static Window? OwnerWindowForDialog( Visual? visual )
	{
		if ( visual?.GetVisualRoot() is Window wRoot )
			return wRoot;
		if ( visual != null && TopLevel.GetTopLevel( visual ) is Window wTop )
			return wTop;
		return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime life ? life.MainWindow : null;
	}
}
