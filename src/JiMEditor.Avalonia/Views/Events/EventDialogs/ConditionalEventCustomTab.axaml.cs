// Ported from: JiMEditor/Views/Interactions/ConditionalInteractionWindow.xaml — Conditional tab (2026-04-14).
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class ConditionalEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	ConditionalInteraction? _conditional;
	bool _suppress;
	/// <summary>Slider Maximum from the last <see cref="RefreshSliderBounds"/> (0 before first run).</summary>
	int _lastSliderMax;

	public ConditionalEventCustomTab()
	{
		InitializeComponent();
		TriggerSelectCombo.SelectionChanged += TriggerSelectCombo_OnSelectionChanged;
	}

	public void Attach( Scenario scenario, ConditionalInteraction conditional )
	{
		Detach();
		_scenario = scenario;
		_conditional = conditional;
		TriggerSelectCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		FinishedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
		SelectedTriggersItems.ItemsSource = conditional.triggerList;
		conditional.triggerList.CollectionChanged += TriggerList_OnCollectionChanged;
	}

	public void Hydrate()
	{
		if ( _conditional == null )
			return;
		SelectTrigger( TriggerSelectCombo, "None" );
		SelectTrigger( FinishedTriggerCombo, _conditional.finishedTrigger );
		RefreshSliderBounds();
	}

	internal void Commit()
	{
		if ( _conditional == null )
			return;
		_conditional.finishedTrigger = FinishedTriggerCombo.SelectedItem is Trigger t ? t.dataName : "None";
		_conditional.triggersRequiredCount = (int)System.Math.Round( TriggersRequiredSlider.Value );
	}

	public void Detach()
	{
		if ( _conditional != null )
			_conditional.triggerList.CollectionChanged -= TriggerList_OnCollectionChanged;
		_conditional = null;
		_scenario = null;
		_lastSliderMax = 0;
	}

	public Control AsControl() => this;

	void TriggerList_OnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e ) => RefreshSliderBounds();

	void RefreshSliderBounds()
	{
		if ( _conditional == null )
			return;

		// Match WPF: Minimum=1, Maximum=trigger count (WPF uses 0 → invalid range; we use max>=1 so One and All are both 1 when empty or single trigger).
		var count = _conditional.triggerList.Count;
		var max = System.Math.Max( 1, count );
		var prevMax = _lastSliderMax;

		var required = _conditional.triggersRequiredCount;
		if ( required < 1 )
			required = 1;
		if ( required > max )
			required = max;
		// If the list grew and the value was "All" for the old size, keep it at the new "All" (WPF binding leaves the int stale).
		if ( prevMax > 0 && max > prevMax && required == prevMax )
			required = max;

		_conditional.triggersRequiredCount = required;

		_suppress = true;
		try
		{
			TriggersRequiredSlider.Minimum = 1;
			TriggersRequiredSlider.Maximum = max;
			TriggersRequiredSlider.TickFrequency = 1;

			// Avalonia: with TickFrequency=0 and empty Ticks, SnapToTick only snaps to Min or Max. Explicit ticks = every integer step.
			var ticks = new AvaloniaList<double>();
			for ( var i = 1; i <= max; i++ )
				ticks.Add( i );
			TriggersRequiredSlider.Ticks = ticks;

			TriggersRequiredSlider.Value = required;
			TriggersRequiredValueText.Text = required.ToString();
		}
		finally
		{
			_suppress = false;
		}

		_lastSliderMax = max;
	}

	static void SelectTrigger( ComboBox cb, string? dataName )
	{
		if ( cb.ItemsSource is not System.Collections.IEnumerable src )
		{
			cb.SelectedItem = null;
			return;
		}
		var list = src.Cast<Trigger>().ToList();
		cb.SelectedItem = list.FirstOrDefault( t => t.dataName == dataName ) ?? list.FirstOrDefault( t => t.dataName == "None" );
	}

	void TriggerSelectCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( TriggerSelectCombo.SelectedItem is Trigger t )
			AddSelectedTriggerButton.IsEnabled = t.dataName != "None";
		else
			AddSelectedTriggerButton.IsEnabled = false;
	}

	void TriggersRequiredSlider_OnValueChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _conditional == null )
			return;
		var value = (int)System.Math.Round( TriggersRequiredSlider.Value );
		_conditional.triggersRequiredCount = value;
		TriggersRequiredValueText.Text = value.ToString();
	}

	void AddSelectedTrigger_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _conditional == null || TriggerSelectCombo.SelectedItem is not Trigger t || t.dataName == "None" )
			return;
		if ( !_conditional.triggerList.Contains( t.dataName ) )
			_conditional.triggerList.Add( t.dataName );
	}

	async void AddTrigger_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _scenario == null || _conditional == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var tw = new TriggerEditorWindow( _scenario );
		await tw.ShowDialog( owner );
		if ( !tw.Accepted || string.IsNullOrWhiteSpace( tw.EnteredTriggerName ) )
			return;
		if ( !_conditional.triggerList.Contains( tw.EnteredTriggerName ) )
			_conditional.triggerList.Add( tw.EnteredTriggerName );
		TriggerSelectCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
		FinishedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
	}

	void RemoveTrigger_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _conditional == null || sender is not Button b || b.CommandParameter is not string trigger )
			return;
		if ( _conditional.triggerList.Contains( trigger ) )
			_conditional.triggerList.Remove( trigger );
	}

	async void AddFinishedTrigger_OnClick( object? sender, RoutedEventArgs e )
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
		FinishedTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
		SelectTrigger( FinishedTriggerCombo, tw.EnteredTriggerName );
	}
}
