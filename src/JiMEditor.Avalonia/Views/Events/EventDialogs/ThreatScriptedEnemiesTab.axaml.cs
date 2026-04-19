using System;
using System.Collections;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.Views;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class ThreatScriptedEnemiesTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	ThreatInteraction? _interaction;
	bool _suppress;

	public event Action<string>? DefeatedTriggerChanged;

	public ThreatScriptedEnemiesTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, ThreatInteraction interaction )
	{
		_scenario = scenario;
		_interaction = interaction;
		ScriptedEnemiesItems.ItemsSource = interaction.monsterCollection;
		DefeatedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
	}

	public void Hydrate()
	{
		if ( _interaction == null || _scenario == null )
			return;
		SetDefeatedTrigger( _interaction.triggerDefeatedName );
	}

	public void Detach()
	{
		_scenario = null;
		_interaction = null;
	}

	public Control AsControl() => this;

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
		finally
		{
			_suppress = false;
		}
	}

	async void AddScriptedEnemyButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction == null || _scenario == null )
			return;
		if ( TopLevel.GetTopLevel( this ) is not Window owner )
			return;
		try
		{
			var editor = new ScriptedEnemyGroupEditorWindow( _scenario );
			await editor.ShowDialog( owner );
			if ( !editor.Accepted )
				return;
			editor.MonsterResult.index = _interaction.monsterCollection.Count + 1;
			_interaction.AddMonster( editor.MonsterResult );
		}
		catch ( Exception ex )
		{
			MessageBox.Show(
				$"Error opening Scripted Enemy Group editor:\n\n{ex.Message}\n\n{ex}",
				"Scripted Enemy Group Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
		}
	}

	async void EditButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button { DataContext: Monster m } || _scenario == null )
			return;
		if ( TopLevel.GetTopLevel( this ) is not Window owner )
			return;
		var editor = new ScriptedEnemyGroupEditorWindow( _scenario, m );
		await editor.ShowDialog( owner );
	}

	async void HelpButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( TopLevel.GetTopLevel( this ) is not Window owner )
			return;
		await HelpWindow.ShowDialogAsync( owner, HelpType.Enemies, 0 );
	}

	void DeleteButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction == null || _scenario == null || sender is not Button { DataContext: Monster m } )
			return;
		var ret = MessageBox.Show( "Are you sure you want to delete this Scripted Enemy?\n\nTHIS CANNOT BE UNDONE.", "Delete Scripted Enemy", MessageBoxButton.YesNo, MessageBoxImage.Question );
		if ( ret != MessageBoxResult.Yes )
			return;
		_interaction.monsterCollection.Remove( m );
		_interaction.RenumberMonsters( _scenario.translationObserver );
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
		var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver ).ToList();
		DefeatedTriggerCombo.ItemsSource = sorted;
		SetDefeatedTrigger( tw.EnteredTriggerName );
		_interaction.triggerDefeatedName = tw.EnteredTriggerName;
		DefeatedTriggerChanged?.Invoke( tw.EnteredTriggerName );
	}
}
