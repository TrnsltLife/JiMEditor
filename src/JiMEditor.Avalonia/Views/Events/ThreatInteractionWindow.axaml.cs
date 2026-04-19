// Ported from: JiMEditor/Views/Interactions/ThreatInteractionWindow.xaml.cs (2026-04-16).
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class ThreatInteractionWindow : BaseEventDialogWindow
{
	readonly ThreatInteraction _threat;
	ThreatScriptedEnemiesTab? _scriptedTab;
	ThreatEnemyPoolTab? _poolTab;

	public ThreatInteractionWindow( Scenario scenario )
		: this( scenario, new ThreatInteraction( "New Threat Event" ), showCancelButton: true )
	{
	}

	public ThreatInteractionWindow( Scenario scenario, ThreatInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.Threat(), showCancelButton )
	{
		_threat = interaction;
		Title = "Enemy Event Editor";
		Loaded += ThreatInteractionWindow_Loaded;
	}

	protected override Control CreateCustomTab()
	{
		_scriptedTab = new ThreatScriptedEnemiesTab();
		_scriptedTab.Attach( Scenario, _threat );
		_scriptedTab.DefeatedTriggerChanged += OnDefeatedTriggerChangedFromScripted;
		return _scriptedTab;
	}

	void ThreatInteractionWindow_Loaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= ThreatInteractionWindow_Loaded;
		var rootTabs = this.FindControl<TabControl>( "RootTabs" );
		if ( rootTabs == null )
			return;

		var customTab = this.FindControl<TabItem>( "CustomTabItem" );
		if ( customTab != null )
			customTab.Header = "Scripted Enemies";

		_poolTab = new ThreatEnemyPoolTab();
		_poolTab.Attach( Scenario, _threat );
		_poolTab.Hydrate();
		_poolTab.DefeatedTriggerChanged += OnDefeatedTriggerChangedFromPool;
		rootTabs.Items.Add( new TabItem
		{
			Header = "Enemy Pool",
			Content = _poolTab,
			Margin = new Thickness( 2, 2, 2, 0 ),
			Padding = new Thickness( 2 )
		} );
	}

	void OnDefeatedTriggerChangedFromScripted( string name ) => _poolTab?.SetDefeatedTrigger( name );
	void OnDefeatedTriggerChangedFromPool( string name ) => _scriptedTab?.SetDefeatedTrigger( name );

	protected override void CommitCustomTabToModel()
	{
		_poolTab?.Commit();
	}
}
