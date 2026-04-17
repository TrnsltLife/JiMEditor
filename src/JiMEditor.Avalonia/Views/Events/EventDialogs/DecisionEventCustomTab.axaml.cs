// Ported from: JiMEditor/Views/Interactions/DecisionInteractionWindow.xaml — Decision tab (2026-04-16).
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class DecisionEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	DecisionInteraction? _decision;
	bool _suppress;

	public DecisionEventCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, DecisionInteraction decision )
	{
		_scenario = scenario;
		_decision = decision;
		var sorted = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		Choice1TriggerCombo.ItemsSource = sorted;
		Choice2TriggerCombo.ItemsSource = sorted;
		Choice3TriggerCombo.ItemsSource = sorted;
	}

	public void Hydrate()
	{
		if ( _decision == null )
			return;
		_suppress = true;
		try
		{
			Choice1TextBox.Text = _decision.choice1 ?? "";
			Choice2TextBox.Text = _decision.choice2 ?? "";
			Choice3TextBox.Text = _decision.choice3 ?? "";
			ThirdChoiceCheckBox.IsChecked = _decision.isThreeChoices;
			SelectTrigger( Choice1TriggerCombo, _decision.choice1Trigger );
			SelectTrigger( Choice2TriggerCombo, _decision.choice2Trigger );
			SelectTrigger( Choice3TriggerCombo, _decision.choice3Trigger );
			SyncThirdChoiceEnabled();
		}
		finally
		{
			_suppress = false;
		}
	}

	internal void Commit()
	{
		if ( _decision == null )
			return;
		_decision.choice1 = Choice1TextBox.Text ?? "";
		_decision.choice2 = Choice2TextBox.Text ?? "";
		_decision.choice3 = Choice3TextBox.Text ?? "";
		_decision.isThreeChoices = ThirdChoiceCheckBox.IsChecked == true;
		_decision.choice1Trigger = GetTriggerName( Choice1TriggerCombo );
		_decision.choice2Trigger = GetTriggerName( Choice2TriggerCombo );
		_decision.choice3Trigger = GetTriggerName( Choice3TriggerCombo );
	}

	public void Detach()
	{
		_scenario = null;
		_decision = null;
	}

	public Control AsControl() => this;

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
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == name ) ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	void SyncThirdChoiceEnabled()
	{
		var enabled = ThirdChoiceCheckBox.IsChecked == true;
		Choice3TextBox.IsEnabled = enabled;
		Choice3TriggerCombo.IsEnabled = enabled;
		AddChoice3TriggerButton.IsEnabled = enabled;
	}

	void ThirdChoiceCheckBox_OnChanged( object? sender, RoutedEventArgs e )
	{
		SyncThirdChoiceEnabled();
		if ( _suppress || _decision == null )
			return;
		_decision.isThreeChoices = ThirdChoiceCheckBox.IsChecked == true;
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
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;

		_suppress = true;
		try
		{
			var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
			target.ItemsSource = sorted;
			SelectTrigger( target, tw.EnteredTriggerName );
		}
		finally
		{
			_suppress = false;
		}

		if ( _decision == null )
			return;
		if ( ReferenceEquals( target, Choice1TriggerCombo ) )
			_decision.choice1Trigger = tw.EnteredTriggerName;
		else if ( ReferenceEquals( target, Choice2TriggerCombo ) )
			_decision.choice2Trigger = tw.EnteredTriggerName;
		else if ( ReferenceEquals( target, Choice3TriggerCombo ) )
			_decision.choice3Trigger = tw.EnteredTriggerName;
	}

	async void AddChoice1TriggerButton_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( Choice1TriggerCombo );
	async void AddChoice2TriggerButton_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( Choice2TriggerCombo );
	async void AddChoice3TriggerButton_OnClick( object? sender, RoutedEventArgs e ) => await AddTriggerForComboAsync( Choice3TriggerCombo );
}
