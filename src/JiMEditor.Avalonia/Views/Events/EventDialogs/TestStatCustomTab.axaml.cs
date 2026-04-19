// Ported from: JiMEditor/Views/Interactions/TestInteractionWindow.xaml — Stat Test tab (2026-04-19).
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class TestStatCustomTab : UserControl, IEventCustomTab
{
	bool _suppress;
	Scenario? _scenario;
	TestInteraction? _test;

	public TestStatCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, TestInteraction test )
	{
		_scenario = scenario;
		_test = test;
		var sorted = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		SuccessTriggerCombo.ItemsSource = sorted;
		FailTriggerCombo.ItemsSource = sorted;
	}

	public void Hydrate()
	{
		if ( _test == null )
			return;
		_suppress = true;
		try
		{
			SetPrimaryRadio( _test.testAttribute );
			SetAltRadio( _test.altTestAttribute );
			NoAlternateCheckBox.IsChecked = _test.noAlternate;
			ProgressiveCheckBox.IsChecked = _test.isCumulative;
			SimplePassCheckBox.IsChecked = _test.passFail;
			SuccessValueTextBox.Text = _test.successValue.ToString( CultureInfo.InvariantCulture );
			RewardLoreTextBox.Text = _test.rewardLore.ToString( CultureInfo.InvariantCulture );
			RewardXPTextBox.Text = _test.rewardXP.ToString( CultureInfo.InvariantCulture );
			RewardThreatTextBox.Text = _test.rewardThreat.ToString( CultureInfo.InvariantCulture );
			FailThreatTextBox.Text = _test.failThreat.ToString( CultureInfo.InvariantCulture );
			SelectTrigger( SuccessTriggerCombo, _test.successTrigger );
			SelectTrigger( FailTriggerCombo, _test.failTrigger );
			SyncAlternateRowEnabled();
			SyncProgressiveDependentUi();
		}
		finally
		{
			_suppress = false;
		}
	}

	internal void Commit()
	{
		if ( _test == null )
			return;
		_test.testAttribute = ReadPrimaryAbility();
		_test.altTestAttribute = ReadAltAbility();
		_test.noAlternate = NoAlternateCheckBox.IsChecked == true;
		_test.isCumulative = ProgressiveCheckBox.IsChecked == true;
		_test.passFail = SimplePassCheckBox.IsChecked == true;
		_test.successValue = ParseInt( SuccessValueTextBox.Text, _test.successValue );
		_test.rewardLore = ParseInt( RewardLoreTextBox.Text, _test.rewardLore );
		_test.rewardXP = ParseInt( RewardXPTextBox.Text, _test.rewardXP );
		_test.rewardThreat = ParseInt( RewardThreatTextBox.Text, _test.rewardThreat );
		_test.failThreat = ParseInt( FailThreatTextBox.Text, _test.failThreat );
		_test.successTrigger = SuccessTriggerCombo.SelectedItem is Trigger tp ? tp.dataName : "None";
		_test.failTrigger = FailTriggerCombo.SelectedItem is Trigger tf ? tf.dataName : "None";
	}

	public void Detach()
	{
		_test = null;
		_scenario = null;
	}

	public Control AsControl() => this;

	void SyncAlternateRowEnabled()
	{
		// When "Don't use Alternate Test attribute" is checked, disable alternate radios (WPF: binv on noAlternate).
		AlternateAttributesPanel.IsEnabled = NoAlternateCheckBox.IsChecked != true;
	}

	void SyncProgressiveDependentUi()
	{
		var progressive = ProgressiveCheckBox.IsChecked == true;
		SimplePassCheckBox.IsEnabled = progressive;
		FailTextButton.IsEnabled = !progressive;
		ProgressTextButton.IsEnabled = progressive;
		PenaltyForFailureBorder.IsEnabled = FailTextButton.IsEnabled;
	}

	void NoAlternateCheckBox_OnChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress )
			return;
		SyncAlternateRowEnabled();
	}

	void ProgressiveCheckBox_OnChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress )
			return;
		SyncProgressiveDependentUi();
	}

	void SetPrimaryRadio( Ability a )
	{
		MightRadio.IsChecked = a == Ability.Might;
		AgilityRadio.IsChecked = a == Ability.Agility;
		SpiritRadio.IsChecked = a == Ability.Spirit;
		WisdomRadio.IsChecked = a == Ability.Wisdom;
		WitRadio.IsChecked = a == Ability.Wit;
	}

	void SetAltRadio( Ability a )
	{
		MightAltRadio.IsChecked = a == Ability.Might;
		AgilityAltRadio.IsChecked = a == Ability.Agility;
		SpiritAltRadio.IsChecked = a == Ability.Spirit;
		WisdomAltRadio.IsChecked = a == Ability.Wisdom;
		WitAltRadio.IsChecked = a == Ability.Wit;
	}

	Ability ReadPrimaryAbility()
	{
		if ( MightRadio.IsChecked == true )
			return Ability.Might;
		if ( AgilityRadio.IsChecked == true )
			return Ability.Agility;
		if ( SpiritRadio.IsChecked == true )
			return Ability.Spirit;
		if ( WisdomRadio.IsChecked == true )
			return Ability.Wisdom;
		if ( WitRadio.IsChecked == true )
			return Ability.Wit;
		return Ability.Might;
	}

	Ability ReadAltAbility()
	{
		if ( MightAltRadio.IsChecked == true )
			return Ability.Might;
		if ( AgilityAltRadio.IsChecked == true )
			return Ability.Agility;
		if ( SpiritAltRadio.IsChecked == true )
			return Ability.Spirit;
		if ( WisdomAltRadio.IsChecked == true )
			return Ability.Wisdom;
		if ( WitAltRadio.IsChecked == true )
			return Ability.Wit;
		return Ability.Might;
	}

	static int ParseInt( string? s, int fallback )
	{
		if ( string.IsNullOrWhiteSpace( s ) )
			return fallback;
		return int.TryParse( s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v )
			? v
			: fallback;
	}

	static void SelectTrigger( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}

		var list = src.Cast<Trigger>().ToList();
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName )
		                  ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	async void PassTextButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await EditBookPageAsync( _test?.passBookData );

	async void FailTextButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await EditBookPageAsync( _test?.failBookData );

	async void ProgressTextButton_OnClick( object? sender, RoutedEventArgs e ) =>
		await EditBookPageAsync( _test?.progressBookData );

	async Task EditBookPageAsync( TextBookData? book )
	{
		if ( book == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var page0 = book.pages.ElementAtOrDefault( 0 ) ?? "";
		var dlg = new StoryTextEditWindow( page0 );
		await dlg.ShowDialog( owner );
		if ( !dlg.Accepted )
			return;
		if ( book.pages.Count == 0 )
			book.pages.Add( dlg.StoryText );
		else
			book.pages[0] = dlg.StoryText;
	}

	async void AddSuccessTriggerButton_OnClick( object? sender, RoutedEventArgs e )
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
		var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver ).ToList();
		SuccessTriggerCombo.ItemsSource = sorted;
		SelectTrigger( SuccessTriggerCombo, tw.EnteredTriggerName );
	}

	async void AddFailTriggerButton_OnClick( object? sender, RoutedEventArgs e )
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
		var sorted = EventDialogTriggerSort.Sort( _scenario.triggersObserver ).ToList();
		FailTriggerCombo.ItemsSource = sorted;
		SelectTrigger( FailTriggerCombo, tw.EnteredTriggerName );
	}
}
