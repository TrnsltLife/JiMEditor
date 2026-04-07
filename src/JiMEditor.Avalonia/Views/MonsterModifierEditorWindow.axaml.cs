// Ported from: JiMEditor/Views/MonsterModifierEditorWindow.xaml.cs (2026-04-06) — Avalonia UI.
using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views;

public partial class MonsterModifierEditorWindow : Window, INotifyPropertyChanged
{
	string _shortName = "";
	readonly bool _readOnly;

	public Scenario scenario { get; private set; } = null!;
	public MonsterModifier modifier { get; private set; } = null!;

	public string shortName
	{
		get => _shortName;
		set
		{
			if ( _shortName == value )
				return;
			_shortName = value;
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( shortName ) ) );
		}
	}

	/// <summary>WPF <c>DialogResult == true</c>.</summary>
	public bool Accepted { get; private set; }

	public MonsterModifierEditorWindow()
	{
		InitializeComponent();
	}

	/// <param name="isNew">
	/// WPF: when <c>false</c>, Cancel is hidden (add flow and edit-in-place). When <c>true</c>, Cancel is shown (duplicate flow).
	/// </param>
	/// <param name="readOnly">When <c>true</c>, all inputs are disabled; OK only closes the window (built-in bonuses).</param>
	public MonsterModifierEditorWindow( Scenario s, MonsterModifier mod, bool isNew = true, bool readOnly = false )
		: this()
	{
		DataContext = this;
		scenario = s;
		modifier = mod;
		_readOnly = readOnly;
		shortName = modifier.name;

		CancelButton.IsVisible = isNew && !readOnly;

		if ( readOnly )
			Title = "Monster Bonus (view only)";

		Loaded += OnMonsterModifierEditorLoaded;
	}

	void OnMonsterModifierEditorLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnMonsterModifierEditorLoaded;
		PopulateFromModifier();
		if ( _readOnly )
			ApplyReadOnlyChrome();
		else
		{
			NameTextBox?.Focus();
			NameTextBox?.SelectAll();
		}
	}

	void ApplyReadOnlyChrome()
	{
		NameTextBox.IsReadOnly = true;
		DataNameTextBox.IsReadOnly = true;
		HealthTextBox.IsReadOnly = true;
		ArmorTextBox.IsReadOnly = true;
		SorceryTextBox.IsReadOnly = true;
		DamageTextBox.IsReadOnly = true;
		FearTextBox.IsReadOnly = true;

		ImmuneCleaveCheck.IsEnabled = false;
		ImmuneLethalCheck.IsEnabled = false;
		ImmunePierceCheck.IsEnabled = false;
		ImmuneSmiteCheck.IsEnabled = false;
		ImmuneStunCheck.IsEnabled = false;
		ImmuneSunderCheck.IsEnabled = false;
		FakeCleaveCheck.IsEnabled = false;
		FakeLethalCheck.IsEnabled = false;
		FakePierceCheck.IsEnabled = false;
		FakeSmiteCheck.IsEnabled = false;
		FakeStunCheck.IsEnabled = false;
		FakeSunderCheck.IsEnabled = false;

		KeyDown += OnReadOnlyKeyDown;
	}

	void OnReadOnlyKeyDown( object? sender, KeyEventArgs e )
	{
		if ( e.Key == Key.Escape )
		{
			e.Handled = true;
			KeyDown -= OnReadOnlyKeyDown;
			Accepted = false;
			Close();
		}
	}

	void PopulateFromModifier()
	{
		DataNameTextBox.Text = modifier.dataName ?? "";
		HealthTextBox.Text = modifier.health.ToString( CultureInfo.InvariantCulture );
		ArmorTextBox.Text = modifier.armor.ToString( CultureInfo.InvariantCulture );
		SorceryTextBox.Text = modifier.sorcery.ToString( CultureInfo.InvariantCulture );
		DamageTextBox.Text = modifier.damage.ToString( CultureInfo.InvariantCulture );
		FearTextBox.Text = modifier.fear.ToString( CultureInfo.InvariantCulture );

		ImmuneCleaveCheck.IsChecked = modifier.immuneCleave;
		ImmuneLethalCheck.IsChecked = modifier.immuneLethal;
		ImmunePierceCheck.IsChecked = modifier.immunePierce;
		ImmuneSmiteCheck.IsChecked = modifier.immuneSmite;
		ImmuneStunCheck.IsChecked = modifier.immuneStun;
		ImmuneSunderCheck.IsChecked = modifier.immuneSunder;

		FakeCleaveCheck.IsChecked = modifier.fakeCleave;
		FakeLethalCheck.IsChecked = modifier.fakeLethal;
		FakePierceCheck.IsChecked = modifier.fakePierce;
		FakeSmiteCheck.IsChecked = modifier.fakeSmite;
		FakeStunCheck.IsChecked = modifier.fakeStun;
		FakeSunderCheck.IsChecked = modifier.fakeSunder;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( _readOnly )
		{
			KeyDown -= OnReadOnlyKeyDown;
			Accepted = false;
			Close();
			return;
		}

		modifier.name = ( shortName ?? "" ).Trim();
		modifier.dataName = ( DataNameTextBox.Text ?? "" ).Trim();

		if ( string.IsNullOrEmpty( modifier.name ) )
		{
			MessageBox.Show( "The Monster Bonus Name cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		if ( string.IsNullOrEmpty( modifier.dataName ) )
			modifier.dataName = modifier.name;

		modifier.health = ParseStatInt( HealthTextBox.Text );
		modifier.armor = ParseStatInt( ArmorTextBox.Text );
		modifier.sorcery = ParseStatInt( SorceryTextBox.Text );
		modifier.damage = ParseStatInt( DamageTextBox.Text );
		modifier.fear = ParseStatInt( FearTextBox.Text );

		modifier.immuneCleave = ImmuneCleaveCheck.IsChecked == true;
		modifier.immuneLethal = ImmuneLethalCheck.IsChecked == true;
		modifier.immunePierce = ImmunePierceCheck.IsChecked == true;
		modifier.immuneSmite = ImmuneSmiteCheck.IsChecked == true;
		modifier.immuneStun = ImmuneStunCheck.IsChecked == true;
		modifier.immuneSunder = ImmuneSunderCheck.IsChecked == true;

		modifier.fakeCleave = FakeCleaveCheck.IsChecked == true;
		modifier.fakeLethal = FakeLethalCheck.IsChecked == true;
		modifier.fakePierce = FakePierceCheck.IsChecked == true;
		modifier.fakeSmite = FakeSmiteCheck.IsChecked == true;
		modifier.fakeStun = FakeStunCheck.IsChecked == true;
		modifier.fakeSunder = FakeSunderCheck.IsChecked == true;

		if ( scenario.IsDuplicate( modifier ) )
		{
			MessageBox.Show( $"A Monster Bonus with In-Editor Name [{modifier.dataName}] already exists.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	static int ParseStatInt( string? text )
	{
		if ( string.IsNullOrWhiteSpace( text ) )
			return 0;
		return int.TryParse( text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n ) ? n : 0;
	}
}
