// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml — Dialog tab (2026-04-13).
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class DialogEventCustomTab : UserControl
{
	bool _suppress;
	Scenario? _scenario;
	DialogInteraction? _dialog;
	PropertyChangedEventHandler? _dialogPropertyHandler;

	public DialogEventCustomTab()
	{
		InitializeComponent();
		C1TriggerCombo.SelectionChanged += (_, _) => SyncChoiceTriggerFromCombo( 1 );
		C2TriggerCombo.SelectionChanged += (_, _) => SyncChoiceTriggerFromCombo( 2 );
		C3TriggerCombo.SelectionChanged += (_, _) => SyncChoiceTriggerFromCombo( 3 );
	}

	public void Attach( Scenario scenario, DialogInteraction dialog )
	{
		DetachDialogPropertyListener();
		_scenario = scenario;
		_dialog = dialog;
		_dialogPropertyHandler = OnDialogPropertyChanged;
		_dialog.PropertyChanged += _dialogPropertyHandler;
		var sorted = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		C1TriggerCombo.ItemsSource = sorted;
		C2TriggerCombo.ItemsSource = sorted;
		C3TriggerCombo.ItemsSource = sorted;
	}

	public void Detach() => DetachDialogPropertyListener();

	void DetachDialogPropertyListener()
	{
		if ( _dialog != null && _dialogPropertyHandler != null )
			_dialog.PropertyChanged -= _dialogPropertyHandler;
		_dialogPropertyHandler = null;
	}

	void OnDialogPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( !string.IsNullOrEmpty( e.PropertyName ) && e.PropertyName != nameof( PersistentInteractionBase.isPersistent ) )
			return;
		if ( _dialog == null )
			return;
		_suppress = true;
		try
		{
			DialogPersistentCheckBox.IsChecked = _dialog.isPersistent;
			EditPersistentDialogButton.IsEnabled = _dialog.isPersistent;
		}
		finally
		{
			_suppress = false;
		}
	}

	public void Hydrate()
	{
		if ( _dialog == null )
			return;
		_suppress = true;
		try
		{
			Choice1TextBox.Text = _dialog.choice1 ?? "";
			Choice2TextBox.Text = _dialog.choice2 ?? "";
			Choice3TextBox.Text = _dialog.choice3 ?? "";
			DialogPersistentCheckBox.IsChecked = _dialog.isPersistent;
			EditPersistentDialogButton.IsEnabled = _dialog.isPersistent;
			SelectTrigger( C1TriggerCombo, _dialog.c1Trigger );
			SelectTrigger( C2TriggerCombo, _dialog.c2Trigger );
			SelectTrigger( C3TriggerCombo, _dialog.c3Trigger );
		}
		finally
		{
			_suppress = false;
		}
	}

	public void Commit()
	{
		if ( _dialog == null )
			return;
		_dialog.choice1 = Choice1TextBox.Text ?? "";
		_dialog.choice2 = Choice2TextBox.Text ?? "";
		_dialog.choice3 = Choice3TextBox.Text ?? "";
		_dialog.isPersistent = DialogPersistentCheckBox.IsChecked == true;
		_dialog.c1Trigger = GetTriggerName( C1TriggerCombo );
		_dialog.c2Trigger = GetTriggerName( C2TriggerCombo );
		_dialog.c3Trigger = GetTriggerName( C3TriggerCombo );
	}

	static string GetTriggerName( ComboBox cb ) =>
		cb.SelectedItem is Trigger t ? t.dataName : "None";

	static void SelectTrigger( ComboBox cb, string? name )
	{
		if ( cb.ItemsSource is not IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}

		var list = src.Cast<Trigger>().ToList();
		if ( string.IsNullOrEmpty( name ) || name == "None" )
		{
			cb.SelectedItem = list.FirstOrDefault( t => t.dataName == "None" );
			return;
		}

		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == name );
	}

	void SyncChoiceTriggerFromCombo( int n )
	{
		if ( _suppress || _dialog == null )
			return;
		switch ( n )
		{
			case 1:
				_dialog.c1Trigger = GetTriggerName( C1TriggerCombo );
				break;
			case 2:
				_dialog.c2Trigger = GetTriggerName( C2TriggerCombo );
				break;
			case 3:
				_dialog.c3Trigger = GetTriggerName( C3TriggerCombo );
				break;
		}
	}

	void DialogPersistentCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _dialog == null )
			return;
		_dialog.isPersistent = DialogPersistentCheckBox.IsChecked == true;
		EditPersistentDialogButton.IsEnabled = DialogPersistentCheckBox.IsChecked == true;
	}

	async Task AddTriggerForComboAsync( ComboBox target )
	{
		if ( _scenario == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted )
			return;
		var name = tw.EnteredTriggerName;
		if ( string.IsNullOrEmpty( name ) )
			return;
		var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
		_suppress = true;
		try
		{
			target.ItemsSource = sorted;
			SelectTrigger( target, name );
		}
		finally
		{
			_suppress = false;
		}

		if ( _dialog == null )
			return;
		if ( ReferenceEquals( target, C1TriggerCombo ) )
			_dialog.c1Trigger = name;
		else if ( ReferenceEquals( target, C2TriggerCombo ) )
			_dialog.c2Trigger = name;
		else if ( ReferenceEquals( target, C3TriggerCombo ) )
			_dialog.c3Trigger = name;
	}

	async void AddC1Trigger_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( C1TriggerCombo );

	async void AddC2Trigger_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( C2TriggerCombo );

	async void AddC3Trigger_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( C3TriggerCombo );

	async Task EditStoryAsync( string current, System.Action<string> write )
	{
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var dlg = new StoryTextEditWindow( current );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
			write( dlg.StoryText );
	}

	async void EditStory1_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _dialog == null )
			return;
		await EditStoryAsync( _dialog.c1Text, s => _dialog.c1Text = s );
	}

	async void EditStory2_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _dialog == null )
			return;
		await EditStoryAsync( _dialog.c2Text, s => _dialog.c2Text = s );
	}

	async void EditStory3_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _dialog == null )
			return;
		await EditStoryAsync( _dialog.c3Text, s => _dialog.c3Text = s );
	}

	async void EditPersistentDialog_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _dialog == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var dlg = new StoryTextEditWindow( _dialog.persistentText );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
			_dialog.persistentText = dlg.StoryText;
	}
}
