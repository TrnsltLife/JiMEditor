// Ported pattern: JiMEditor interaction event editors — shared shell (2026-04-13).
using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.Views.Events;

/// <summary>
/// Shared interaction-event shell. Avalonia requires a public parameterless constructor for the <c>x:Class</c> type;
/// that ctor is for XAML/designer only — use <see cref="BaseEventDialogWindow(Scenario, InteractionBase, EventDialogConfig, bool)"/> at runtime.
/// </summary>
public partial class BaseEventDialogWindow : Window
{
	protected readonly Scenario Scenario;
	protected readonly InteractionBase Interaction;
	protected readonly EventDialogConfig Config;
	readonly string _oldName;
	bool _allowClose;
	bool _loaded;
	PropertyChangedEventHandler? _interactionPropHandler;
	IEventCustomTab? _customTab;

	/// <summary>Parameterless ctor required by Avalonia XAML for <c>x:Class</c>; not used for real editors.</summary>
	public BaseEventDialogWindow()
	{
		InitializeComponent();
		Scenario = null!;
		Interaction = null!;
		Config = new EventDialogConfig();
		_oldName = "";
	}

	protected BaseEventDialogWindow(
		Scenario scenario,
		InteractionBase interaction,
		EventDialogConfig config,
		bool showCancelButton )
	{
		InitializeComponent();
		Scenario = scenario;
		Interaction = interaction;
		Config = config;
		_oldName = interaction.dataName;
		CancelButton.IsVisible = showCancelButton;
		CustomTabItem.Header = config.CustomTabTitle;
		Loaded += OnWindowLoaded;
	}

	public bool Accepted { get; private set; }

	protected virtual Control CreateCustomTab() => new Border();

	protected virtual void CommitCustomTabToModel()
	{
	}

	public void SetEventTextSectionEnabled( bool enabled ) => TextFlavorSection.SetEventTextEnabled( enabled );

	void OnWindowLoaded( object? sender, RoutedEventArgs e )
	{
		if ( _loaded )
			return;
		if ( Interaction is null || Scenario is null )
		{
			Loaded -= OnWindowLoaded;
			_loaded = true;
			return;
		}

		_loaded = true;
		Loaded -= OnWindowLoaded;

		ApplyConfigVisibility();
		NameGroupSection.Attach( Interaction );
		RewardsSection.Attach( Interaction );
		TokenSection.Attach( Scenario, Interaction, IsThreatTriggered() );
		TriggerSection.Attach( Scenario, Interaction, Config.TriggerHeadingText, Config.TriggerBodyText );
		TextFlavorSection.Attach(
			Interaction,
			Config.FlavorReadOnly,
			Config.EventText == EventSectionVis.Disabled,
			Config.EventBodyText,
			Config.FlavorBodyText );

		NameGroupSection.Hydrate();
		RewardsSection.Hydrate();
		TokenSection.Hydrate();
		TriggerSection.Hydrate();

		if ( IsThreatTriggered() )
		{
			Interaction.isTokenInteraction = false;
			TokenSection.Hydrate();
		}

		SyncTriggeredByEnabledFromModel();
		_interactionPropHandler = OnInteractionPropertyChanged;
		Interaction.PropertyChanged += _interactionPropHandler;
		var customControl = CreateCustomTab();
		_customTab = customControl as IEventCustomTab;
		CustomTabHost.Content = customControl;
		_customTab?.Hydrate();

		ApplyScenarioCollectionAvailability();

		NameGroupSection.NameTextBox.Focus();
		NameGroupSection.NameTextBox.SelectAll();
	}

	/// <summary>
	/// Applies scenario collection rules to token UI, custom tab content (items/titles), and derived editors.
	/// Threat enemy pool tab calls <see cref="ThreatEnemyPoolTab.ApplyScenarioCollectionAvailability"/> after hydrate (see <see cref="ThreatInteractionWindow"/>).
	/// </summary>
	protected virtual void ApplyScenarioCollectionAvailability()
	{
		TokenSection.ApplyScenarioCollectionAvailability( Scenario );
		_customTab?.ApplyScenarioCollectionAvailability( Scenario );
	}

	bool IsThreatTriggered() =>
		Scenario != null
		&& Interaction != null
		&& Scenario.threatObserver.Any( x => x.triggerName == Interaction.dataName );

	void ApplyConfigVisibility()
	{
		if ( Config.ThreatCallout == EventSectionVis.Collapsed || !IsThreatTriggered() )
			ThreatCalloutBorder.IsVisible = false;
		else
		{
			ThreatCalloutBorder.IsVisible = true;
			ThreatCalloutBorder.IsEnabled = Config.ThreatCallout != EventSectionVis.Disabled;
		}

		EventDialogSectionHelper.ApplyVisibility( NameGroupSection, Config.EventNameGroup );
		InstructionsHost.Content = Config.InstructionsFactory?.Invoke();
		var instructionsVis = InstructionsHost.Content == null ? EventSectionVis.Collapsed : Config.Instructions;
		EventDialogSectionHelper.ApplyVisibility( InstructionsHost, instructionsVis );
		EventDialogSectionHelper.ApplyVisibility( RewardsSection, Config.Rewards );
		TokenOverrideHost.Content = Config.TokenSectionOverrideFactory?.Invoke();
		EventDialogSectionHelper.ApplyVisibility(
			TokenOverrideHost,
			TokenOverrideHost.Content == null ? EventSectionVis.Collapsed : EventSectionVis.Visible );
		EventDialogSectionHelper.ApplyVisibility( TokenSection, Config.Token );
		TriggerOverrideHost.Content = Config.TriggerSectionOverrideFactory?.Invoke();
		EventDialogSectionHelper.ApplyVisibility(
			TriggerOverrideHost,
			TriggerOverrideHost.Content == null ? EventSectionVis.Collapsed : EventSectionVis.Visible );
		EventDialogSectionHelper.ApplyVisibility( TriggerSection, Config.Triggers );
		EventDialogSectionHelper.ApplyVisibility( TextFlavorSection, Config.EventText );
		CustomTabItem.IsVisible = Config.ShowCustomTab;
	}

	void OnInteractionPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName == nameof( InteractionBase.isTokenInteraction ) )
			SyncTriggeredByEnabledFromModel();
	}

	void SyncTriggeredByEnabledFromModel()
	{
		if ( Interaction != null )
			TriggerSection.SetTriggeredByEnabled( !Interaction.isTokenInteraction );
	}

	protected override void OnClosing( WindowClosingEventArgs e )
	{
		if ( !_allowClose )
		{
			e.Cancel = true;
			return;
		}

		if ( _interactionPropHandler != null )
		{
			var model = Interaction;
			if ( model != null )
				model.PropertyChanged -= _interactionPropHandler;
			_interactionPropHandler = null;
		}

		TokenSection.Detach();
		_customTab?.Detach();

		base.OnClosing( e );
	}

	void OkButton_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( Interaction is null || Scenario is null )
		{
			_allowClose = true;
			Close();
			return;
		}

		RewardsSection.Commit();
		TokenSection.Commit();
		CommitCustomTabToModel();
		NameGroupSection.CommitNameToModel();

		if ( !TryValidateForOk() )
			return;

		Scenario.UpdateEventReferences( _oldName, Interaction );
		_allowClose = true;
		Accepted = true;
		Close();
	}

	protected virtual bool TryValidateForOk()
	{
		if ( Interaction is null || Scenario is null )
			return false;

		var forbidden = Config.ForbiddenDefaultEventName;
		if ( !string.IsNullOrEmpty( forbidden ) && Interaction.dataName == forbidden )
		{
			MessageBox.Show(
				"An Event with this name already exists. Give this Event a unique name.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		if ( Scenario.interactionObserver.Count( x => x.dataName == Interaction.dataName && x.GUID != Interaction.GUID ) > 0 )
		{
			MessageBox.Show(
				"An Event with this name already exists. Give this Event a unique name.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		return true;
	}

	void CancelButton_OnClick( object? sender, RoutedEventArgs e )
	{
		_allowClose = true;
		Accepted = false;
		Close();
	}
}
