// Ported from: JiMEditor/Views/MonsterActivationItemEditorWindow.xaml.cs (2026-04-07) — simplified Avalonia UI.
using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views;

public partial class MonsterActivationItemEditorWindow : Window
{
	string _originalText = "";
	string _originalEffect = "";
	readonly bool _readOnly;

	public Scenario scenario { get; private set; } = null!;
	public MonsterActivations activations { get; private set; } = null!;
	public MonsterActivationItem activationItem { get; private set; } = null!;

	public bool Accepted { get; private set; }

	public MonsterActivationItemEditorWindow()
	{
		InitializeComponent();
	}

	public MonsterActivationItemEditorWindow( Scenario s, MonsterActivations act, MonsterActivationItem actItem, bool isNew = true, bool readOnly = false )
		: this()
	{
		scenario = s;
		activations = act;
		activationItem = actItem;
		_readOnly = readOnly;
		_originalText = actItem.text ?? "";
		_originalEffect = actItem.effect ?? "";

		CancelButton.IsVisible = isNew && !readOnly;
		NegateComboBox.ItemsSource = Enum.GetValues<Ability>().ToList();
		DataContext = this;

		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		AttackTextBox.Text = activationItem.text ?? "";
		AfterEffectTextBox.Text = activationItem.effect ?? "";
		Damage1TextBox.Text = activationItem.damage[0].ToString( CultureInfo.InvariantCulture );
		Damage2TextBox.Text = activationItem.damage[1].ToString( CultureInfo.InvariantCulture );
		Damage3TextBox.Text = activationItem.damage[2].ToString( CultureInfo.InvariantCulture );
		Fear1TextBox.Text = activationItem.fear[0].ToString( CultureInfo.InvariantCulture );
		Fear2TextBox.Text = activationItem.fear[1].ToString( CultureInfo.InvariantCulture );
		Fear3TextBox.Text = activationItem.fear[2].ToString( CultureInfo.InvariantCulture );
		NegateComboBox.SelectedItem = activationItem.negate;
		if ( _readOnly )
			ApplyReadOnlyChrome();
		else
			AttackTextBox.Focus();
	}

	void ApplyReadOnlyChrome()
	{
		AttackTextBox.IsEnabled = false;
		AfterEffectTextBox.IsEnabled = false;
		Damage1TextBox.IsEnabled = false;
		Damage2TextBox.IsEnabled = false;
		Damage3TextBox.IsEnabled = false;
		Fear1TextBox.IsEnabled = false;
		Fear2TextBox.IsEnabled = false;
		Fear3TextBox.IsEnabled = false;
		NegateComboBox.IsEnabled = false;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( _readOnly )
		{
			Accepted = false;
			Close();
			return;
		}

		var text = ( AttackTextBox.Text ?? "" ).Trim();
		if ( string.IsNullOrEmpty( text ) )
		{
			MessageBox.Show( "The Attack Text cannot be empty.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return;
		}

		activationItem.text = text;
		activationItem.effect = ( AfterEffectTextBox.Text ?? "" ).Trim();
		activationItem.damage[0] = ParseInt( Damage1TextBox.Text );
		activationItem.damage[1] = ParseInt( Damage2TextBox.Text );
		activationItem.damage[2] = ParseInt( Damage3TextBox.Text );
		activationItem.fear[0] = ParseInt( Fear1TextBox.Text );
		activationItem.fear[1] = ParseInt( Fear2TextBox.Text );
		activationItem.fear[2] = ParseInt( Fear3TextBox.Text );
		activationItem.negate = NegateComboBox.SelectedItem is Ability ability ? ability : Ability.None;

		if ( activationItem.text != _originalText )
			activationItem.NotifyPropertyChanged( nameof( activationItem.text ) );
		if ( activationItem.effect != _originalEffect )
			activationItem.NotifyPropertyChanged( nameof( activationItem.effect ) );

		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	static int ParseInt( string? text ) =>
		int.TryParse( text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n ) ? n : 0;
}
