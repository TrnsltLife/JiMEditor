using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class ScriptedEnemyGroupEditorWindow : Window
{
	readonly Scenario _scenario;
	readonly Monster _monster;
	bool _isInitializing = true;
	int _lastSelectedMonsterTypeId;

	public bool Accepted { get; private set; }
	public Monster MonsterResult => _monster;

	public ScriptedEnemyGroupEditorWindow()
	{
		InitializeComponent();
		_scenario = null!;
		_monster = new Monster( 1 );
	}

	public ScriptedEnemyGroupEditorWindow( Scenario scenario ) : this( scenario, null )
	{
	}

	public ScriptedEnemyGroupEditorWindow( Scenario scenario, Monster? source )
	{
		InitializeComponent();
		_scenario = scenario;
		_monster = source ?? new Monster( 1 ) { count = 1, isEasy = true, isNormal = true, isHard = true, defaultStats = true };
		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		ActivationCombo.ItemsSource = _scenario.activationsObserver;
		ModifierCombo.ItemsSource = _scenario.monsterModifierObserver;
		ModifiersItems.ItemsSource = _monster.modifierList;
		FillUiFromMonster();
		_isInitializing = false;
	}

	void FillUiFromMonster()
	{
		NameTextBox.Text = _monster.dataName ?? "";
		CountCombo.SelectedIndex = _monster.count <= 1 ? 0 : _monster.count - 1;
		LoreRewardTextBox.Text = _monster.loreReward.ToString( CultureInfo.InvariantCulture );
		UseDefaultCheckBox.IsChecked = _monster.defaultStats;
		HealthTextBox.Text = _monster.health.ToString( CultureInfo.InvariantCulture );
		DamageTextBox.Text = _monster.damage.ToString( CultureInfo.InvariantCulture );
		ShieldTextBox.Text = _monster.shieldValue.ToString( CultureInfo.InvariantCulture );
		SorceryTextBox.Text = _monster.sorceryValue.ToString( CultureInfo.InvariantCulture );
		MoveATextBox.Text = _monster.moveA.ToString( CultureInfo.InvariantCulture );
		MoveBTextBox.Text = _monster.moveB.ToString( CultureInfo.InvariantCulture );
		IsRangedCheckBox.IsChecked = _monster.isRanged;
		ActivationCombo.SelectedItem = _scenario.activationsObserver.FirstOrDefault( a => a.id == _monster.activationsId ) ?? _scenario.activationsObserver.FirstOrDefault();
		EasyCheckBox.IsChecked = _monster.isEasy;
		NormalCheckBox.IsChecked = _monster.isNormal;
		HardCheckBox.IsChecked = _monster.isHard;
		RandomizedModifiersSlider.Maximum = System.Math.Max( 1, _monster.modifierList.Count == 0 ? 1 : _monster.modifierList.Count );
		RandomizedModifiersSlider.Value = _monster.randomizedModifiersCount <= 0 ? 1 : _monster.randomizedModifiersCount;
		RandomizedCountText.Text = ( (int)RandomizedModifiersSlider.Value ).ToString( CultureInfo.InvariantCulture );
		SetSelectedMonsterType( (int)_monster.monsterType );
		_lastSelectedMonsterTypeId = (int)_monster.monsterType;
		ApplyDefaultStatsEnabledState();
		UpdateSpecialText();
	}

	void SetSelectedMonsterType( int id )
	{
		var target = MonsterTypeButtons().FirstOrDefault( rb => rb.Tag?.ToString() == id.ToString( CultureInfo.InvariantCulture ) );
		( target ?? GoblinScoutRadio ).IsChecked = true;
	}

	// Name updates when empty or still the default for previousTypeId (WPF MonsterEditorWindow).
	void SyncStatsFromMonsterType( int newTypeId, int previousTypeIdForNameRule )
	{
		var seeded = new Monster( newTypeId );
		HealthTextBox.Text = seeded.health.ToString( CultureInfo.InvariantCulture );
		DamageTextBox.Text = seeded.damage.ToString( CultureInfo.InvariantCulture );
		ShieldTextBox.Text = seeded.shieldValue.ToString( CultureInfo.InvariantCulture );
		SorceryTextBox.Text = seeded.sorceryValue.ToString( CultureInfo.InvariantCulture );
		MoveATextBox.Text = seeded.moveA.ToString( CultureInfo.InvariantCulture );
		MoveBTextBox.Text = seeded.moveB.ToString( CultureInfo.InvariantCulture );
		IsRangedCheckBox.IsChecked = seeded.isRanged;
		ActivationCombo.SelectedItem = _scenario.activationsObserver.FirstOrDefault( a => a.id == newTypeId ) ?? _scenario.activationsObserver.FirstOrDefault();
		_monster.special = seeded.special;
		UpdateSpecialText();
		var nameTrim = NameTextBox.Text?.Trim() ?? "";
		var prevDefault = DefaultDataNameForMonsterTypeId( previousTypeIdForNameRule );
		if ( string.IsNullOrEmpty( nameTrim )
		     || ( prevDefault != null && string.Equals( nameTrim, prevDefault, StringComparison.Ordinal ) ) )
			NameTextBox.Text = seeded.dataName;
	}

	static string? DefaultDataNameForMonsterTypeId( int monsterTypeId )
	{
		var monsters = Collection.MONSTERS();
		if ( monsterTypeId < 0 || monsterTypeId >= monsters.Length )
			return null;
		return monsters[monsterTypeId].dataName;
	}

	void ApplyDefaultStatsEnabledState()
	{
		var useDefault = UseDefaultCheckBox.IsChecked == true;
		Stats1Panel.IsEnabled = !useDefault;
		Stats2Panel.IsEnabled = !useDefault;
	}

	void UpdateSpecialText()
	{
		SpecialTextBlock.Text = string.Join( ", ", _monster.special ?? new string[0] );
	}

	void UseDefaultCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		ApplyDefaultStatsEnabledState();
		if ( UseDefaultCheckBox.IsChecked == true )
			SyncStatsFromMonsterType( SelectedMonsterTypeId(), _lastSelectedMonsterTypeId );
	}

	void MonsterTypeRadio_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _isInitializing )
			return;
		if ( sender is not RadioButton { IsChecked: true } selectedRadio )
			return;
		if ( !int.TryParse( selectedRadio.Tag?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var selectedId ) )
			selectedId = SelectedMonsterTypeId();
		var previousTypeId = _lastSelectedMonsterTypeId;
		SyncStatsFromMonsterType( selectedId, previousTypeId );
		_lastSelectedMonsterTypeId = selectedId;
	}

	void ModifierCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		var mod = ModifierCombo.SelectedItem as MonsterModifier;
		AddSelectedModifierButton.IsEnabled = mod is { id: not 0 };
		if ( mod != null )
			UpdateModifierDescription( mod );
	}

	void AddSelectedModifierButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( ModifierCombo.SelectedItem is not MonsterModifier mod )
			return;
		if ( _monster.modifierList.Count >= 7 )
		{
			ModifierDescriptionText.Text = "Maximum of 7 modifiers allowed.";
			return;
		}
		if ( !_monster.modifierList.Contains( mod ) && mod.id != 0 )
		{
			_monster.modifierList.Add( mod );
			_monster.updateElite();
			RandomizedModifiersSlider.Maximum = _monster.modifierList.Count;
		}
	}

	async void AddModifierButton_OnClick( object? sender, RoutedEventArgs e )
	{
		var editor = new MonsterModifierEditorWindow( _scenario, new MonsterModifier() );
		await editor.ShowDialog( this );
		if ( !editor.Accepted )
			return;
		_scenario.AddMonsterModifier( editor.modifier );
		if ( !_monster.modifierList.Contains( editor.modifier ) )
			_monster.modifierList.Add( editor.modifier );
		_monster.updateElite();
		ModifierCombo.ItemsSource = _scenario.monsterModifierObserver;
		RandomizedModifiersSlider.Maximum = _monster.modifierList.Count;
	}

	void RemoveModifierButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button { DataContext: MonsterModifier mod } )
			return;
		_monster.modifierList.Remove( mod );
		_monster.updateElite();
		RandomizedModifiersSlider.Maximum = System.Math.Max( 1, _monster.modifierList.Count );
	}

	void RandomizedModifiersSlider_OnValueChanged( object? sender, RoutedEventArgs e )
	{
		if ( _isInitializing )
			return;
		if ( RandomizedCountText == null )
			return;
		var value = (int)System.Math.Round( RandomizedModifiersSlider.Value );
		_monster.randomizedModifiersCount = value;
		RandomizedCountText.Text = value.ToString( CultureInfo.InvariantCulture );
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		var id = SelectedMonsterTypeId();
		var seeded = new Monster( id );
		_monster.id = seeded.id;
		_monster.monsterType = seeded.monsterType;
		_monster.groupLimit = seeded.groupLimit;
		_monster.figureLimit = seeded.figureLimit;
		_monster.defaultStats = UseDefaultCheckBox.IsChecked == true;
		_monster.dataName = string.IsNullOrWhiteSpace( NameTextBox.Text ) ? seeded.dataName : NameTextBox.Text!.Trim();
		_monster.count = CountCombo.SelectedIndex + 1;
		_monster.loreReward = ParseInt( LoreRewardTextBox.Text, 0 );
		_monster.health = ParseInt( HealthTextBox.Text, seeded.health );
		_monster.damage = ParseInt( DamageTextBox.Text, seeded.damage );
		_monster.shieldValue = ParseInt( ShieldTextBox.Text, seeded.shieldValue );
		_monster.sorceryValue = ParseInt( SorceryTextBox.Text, seeded.sorceryValue );
		_monster.moveA = ParseInt( MoveATextBox.Text, seeded.moveA );
		_monster.moveB = ParseInt( MoveBTextBox.Text, seeded.moveB );
		_monster.isRanged = IsRangedCheckBox.IsChecked == true;
		_monster.activationsId = ( ActivationCombo.SelectedItem as MonsterActivations )?.id ?? seeded.activationsId;
		_monster.isEasy = EasyCheckBox.IsChecked == true;
		_monster.isNormal = NormalCheckBox.IsChecked == true;
		_monster.isHard = HardCheckBox.IsChecked == true;
		_monster.randomizedModifiersCount = (int)RandomizedModifiersSlider.Value;
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	int SelectedMonsterTypeId()
	{
		var selected = MonsterTypeButtons().FirstOrDefault( rb => rb.IsChecked == true );
		return int.TryParse( selected?.Tag?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id ) ? id : 1;
	}

	void UpdateModifierDescription( MonsterModifier mod )
	{
		var mods = new List<string>();
		if ( mod.health != 0 ) mods.Add( ( mod.health > 0 ? "+" : "" ) + mod.health + " Health" );
		if ( mod.armor != 0 ) mods.Add( ( mod.armor > 0 ? "+" : "" ) + mod.armor + " Armor" );
		if ( mod.sorcery != 0 ) mods.Add( ( mod.sorcery > 0 ? "+" : "" ) + mod.sorcery + " Sorcery" );
		if ( mod.damage != 0 ) mods.Add( ( mod.damage > 0 ? "+" : "" ) + mod.damage + " Damage" );
		if ( mod.fear != 0 ) mods.Add( ( mod.fear > 0 ? "+" : "" ) + mod.fear + " Fear" );
		ModifierDescriptionText.Text = mod.name + ": " + string.Join( "; ", mods );
	}

	static int ParseInt( string? text, int fallback ) =>
		int.TryParse( text ?? "", NumberStyles.Integer, CultureInfo.InvariantCulture, out var n ) ? n : fallback;

	IEnumerable<RadioButton> MonsterTypeButtons()
	{
		yield return GoblinScoutRadio;
		yield return HillTrollRadio;
		yield return OrcHunterRadio;
		yield return OrcMarauderRadio;
		yield return RuffianRadio;
		yield return HungryVargRadio;
		yield return WightRadio;
		yield return AtariRadio;
		yield return ChartoothRadio;
		yield return GargletargRadio;
		yield return BalerockRadio;
		yield return CaveTrollRadio;
		yield return GiantSpiderRadio;
		yield return AnonymousThingRadio;
		yield return OrcTaskmasterRadio;
		yield return PitGoblinRadio;
		yield return ShadowmanRadio;
		yield return SpawnOfUglygiantRadio;
		yield return OliverRadio;
		yield return UrsulaRadio;
		yield return SupplicantRadio;
		yield return FoulBeastRadio;
		yield return SiegeEngineRadio;
		yield return SoldierRadio;
		yield return HighOrcWarriorRadio;
		yield return WarElephantRadio;
		yield return VargRiderRadio;
		yield return LordJavelinRadio;
		yield return EndrisRadio;
		yield return LichKingRadio;
	}
}
