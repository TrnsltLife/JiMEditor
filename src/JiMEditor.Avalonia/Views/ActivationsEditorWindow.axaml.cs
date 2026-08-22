// Ported from: JiMEditor/Views/ActivationsEditorWindow.xaml.cs (2026-04-07) — simplified Avalonia UI.
using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class ActivationsEditorWindow : Window, INotifyPropertyChanged
{
	string _oldName = "";
	readonly bool _readOnly;

	public Scenario scenario { get; private set; } = null!;
	public MonsterActivations activations { get; private set; } = null!;
	public bool Accepted { get; private set; }
	public bool CanDeleteAttackDescriptions => !_readOnly;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ActivationsEditorWindow()
	{
		InitializeComponent();
	}

	public ActivationsEditorWindow( Scenario s, MonsterActivations? activ = null, bool isNew = true, bool readOnly = false )
		: this()
	{
		scenario = s;
		activations = activ ?? new MonsterActivations();
		_readOnly = readOnly;
		_oldName = activations.dataName;

		CancelButton.IsVisible = isNew && !readOnly;
		bool isLockedDefault = activ != null && activ.id < MonsterActivations.START_OF_CUSTOM_ACTIVATIONS;
		NameTextBox.IsEnabled = !isLockedDefault && !readOnly;
		AddActivationButton.IsEnabled = !readOnly;
		LockText.IsVisible = isLockedDefault;
		DataContext = this;

		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		NameTextBox.Text = activations.dataName ?? "";
		NameTextBox.TextChanged += NameTextBox_TextChanged;
		RefreshItems();
		if ( !_readOnly )
		{
			NameTextBox.Focus();
			NameTextBox.SelectAll();
		}
	}

	bool TryClosing()
	{
		if ( activations.dataName == "New Enemy Attack Group"
		     || scenario.activationsObserver.Count( x => x.dataName == activations.dataName && x.GUID != activations.GUID ) > 0 )
		{
			MessageBox.Show( "Give this Enemy Attack Group a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
			return false;
		}
		return true;
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( _readOnly )
		{
			Accepted = false;
			Close();
			return;
		}
		if ( !TryClosing() )
			return;
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	async void AddActivationButton_Click( object? sender, RoutedEventArgs e )
	{
		var mai = new MonsterActivationItem( activations.activations.Count + 1 );
		var maie = new MonsterActivationItemEditorWindow( scenario, activations, mai, true );
		await maie.ShowDialog( this );
		if ( !maie.Accepted )
			return;
		activations.activations.Add( mai );
		NotifyItemChanged( mai );
		RefreshItems();
	}

	async void EditButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button { Tag: MonsterActivationItem mai } )
			return;

		if ( _readOnly )
		{
			var ro = new MonsterActivationItemEditorWindow( scenario, activations, mai, false, readOnly: true );
			await ro.ShowDialog( this );
			return;
		}

		var originalValues = mai.CaptureStartingValues();
		var originalPrefix = mai.TranslationKeyPrefix();
		var originalKeyName = mai.TranslationKeyName();
		var maie = new MonsterActivationItemEditorWindow( scenario, activations, mai, false );
		await maie.ShowDialog( this );
		mai.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		NotifyItemChanged( mai );
		RefreshItems();
	}

	void DeleteButton_Click( object? sender, RoutedEventArgs e )
	{
		if ( _readOnly )
			return;
		var ret = MessageBox.Show(
			"Are you sure you want to delete this Enemy Attack Description?\n\nTHIS CANNOT BE UNDONE.",
			"Delete Enemy Attack Description", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;
		if ( sender is not Button { Tag: MonsterActivationItem mai } )
			return;

		mai.UpdateKeysStartingWith(
			scenario.translationObserver,
			activations.TranslationKeyPrefix() + mai.id + ".",
			activations.TranslationKeyPrefix() + "deleted." + mai.GUID + "." );
		activations.activations.Remove( mai );
		activations.RenumberActivations( scenario.translationObserver );
		RefreshItems();
	}

	void NotifyItemChanged( MonsterActivationItem mai )
	{
		mai.NotifyPropertyChanged( "damage" );
		mai.NotifyPropertyChanged( "fear" );
		mai.UpdateValid();
	}

	void NameTextBox_TextChanged( object? sender, TextChangedEventArgs e ) =>
		activations.dataName = NameTextBox.Text ?? _oldName;

	void RefreshItems()
	{
		ActivationItemsControl.ItemsSource = null;
		ActivationItemsControl.ItemsSource = activations.activations;
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( activations ) ) );
	}
}
