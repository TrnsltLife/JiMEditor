// Ported from: JiMEditor/Views/Interactions/PersistentInteractionWindow.xaml — Persistent tab (2026-04-14).
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class PersistentEventCustomTab : UserControl, IEventCustomTab
{
	Scenario? _scenario;
	PersistentTokenInteraction? _persistent;

	public PersistentEventCustomTab()
	{
		InitializeComponent();
	}

	public void Attach( Scenario scenario, PersistentTokenInteraction persistent )
	{
		_scenario = scenario;
		_persistent = persistent;
		EventToActivateCombo.ItemsSource = BuildEventOptions( scenario, persistent ).ToArray();
		AltTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( scenario.triggersObserver );
	}

	static IEnumerable<string> BuildEventOptions( Scenario scenario, PersistentTokenInteraction current ) =>
		scenario.interactionObserver
			.Where( x => x.isTokenInteraction && x.dataName != current.dataName && !Regex.IsMatch( x.dataName, @"\sGRP\d+$" ) )
			.Select( x => x.dataName )
			.Prepend( "None" )
			.Distinct()
			.OrderBy( x => x == "None" ? 0 : 1 )
			.ThenBy( x => x );

	public void Hydrate()
	{
		if ( _persistent == null )
			return;
		SelectString( EventToActivateCombo, _persistent.eventToActivate );
		SelectTrigger( AltTriggerCombo, _persistent.alternativeTextTrigger );
	}

	internal void Commit()
	{
		if ( _persistent == null )
			return;
		_persistent.eventToActivate = EventToActivateCombo.SelectedItem as string ?? "None";
		_persistent.alternativeTextTrigger = AltTriggerCombo.SelectedItem is Trigger t ? t.dataName : "None";
	}

	public void Detach()
	{
	}

	public Control AsControl() => this;

	static void SelectString( ComboBox cb, string? value )
	{
		if ( cb.ItemsSource is not IEnumerable<string> src )
			return;
		var selected = src.FirstOrDefault( x => x == value ) ?? "None";
		cb.SelectedItem = selected;
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

	async void AddAltTrigger_OnClick( object? sender, RoutedEventArgs e )
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
		AltTriggerCombo.ItemsSource = EventDialogTriggerSort.Sort( _scenario.triggersObserver );
		SelectTrigger( AltTriggerCombo, tw.EnteredTriggerName );
	}

	async void EditAltText_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _persistent == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var page0 = _persistent.alternativeBookData.pages.ElementAtOrDefault( 0 ) ?? "";
		var dlg = new StoryTextEditWindow( page0 );
		await dlg.ShowDialog( owner );
		if ( !dlg.Accepted )
			return;
		if ( _persistent.alternativeBookData.pages.Count == 0 )
			_persistent.alternativeBookData.pages.Add( dlg.StoryText );
		else
			_persistent.alternativeBookData.pages[0] = dlg.StoryText;
	}
}
