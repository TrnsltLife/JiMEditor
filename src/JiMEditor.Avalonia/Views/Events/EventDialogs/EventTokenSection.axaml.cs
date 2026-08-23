// Ported from: JiMEditor/UserControls/TokenTypeSelector.xaml.cs — Avalonia subset (2026-04-13).
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia.EventDialog;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventTokenSection : UserControl
{
	bool _suppress;
	bool _isThreatTriggered;
	Scenario? _scenario;
	InteractionBase? _interaction;
	PropertyChangedEventHandler? _interactionPropertyHandler;

	public EventTokenSection()
	{
		InitializeComponent();
		PersonTypeCombo.ItemsSource = new[] { PersonType.Human, PersonType.Elf, PersonType.Hobbit, PersonType.Dwarf };
		TerrainTypeCombo.ItemsSource = Array.Empty<TerrainType>();
	}

	public void Attach( Scenario scenario, InteractionBase interaction, bool isThreatTriggered )
	{
		DetachInteractionPropertyListener();
		_scenario = scenario;
		_interaction = interaction;
		_isThreatTriggered = isThreatTriggered;
		IsTokenInteractionCheckBox.IsEnabled = !isThreatTriggered;
		PopulateTokenTypeCombo();
		if ( _interaction is INotifyPropertyChanged inpc )
		{
			_interactionPropertyHandler = OnInteractionPropertyChanged;
			inpc.PropertyChanged += _interactionPropertyHandler;
		}
	}

	public void Detach() => DetachInteractionPropertyListener();

	void DetachInteractionPropertyListener()
	{
		if ( _interaction != null && _interactionPropertyHandler != null )
			_interaction.PropertyChanged -= _interactionPropertyHandler;
		_interactionPropertyHandler = null;
	}

	void OnInteractionPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( !string.IsNullOrEmpty( e.PropertyName ) && e.PropertyName != nameof( PersistentInteractionBase.isPersistent ) )
			return;
		if ( _interaction is TextInteraction )
			return;
		if ( _interaction is not PersistentInteractionBase p )
			return;
		_suppress = true;
		try
		{
			TokenPersistentCheckBox.IsChecked = p.isPersistent;
			EditPersistentFromTokenButton.IsEnabled = p.isPersistent;
		}
		finally
		{
			_suppress = false;
		}
	}

	void PopulateTokenTypeCombo()
	{
		if ( _scenario == null || _interaction == null )
			return;
		var list = ScenarioCollectionAvailability.GetSelectableTokenTypes( _scenario, _scenario.scenarioTypeJourney ).ToList();
		var current = _interaction.tokenType;
		if ( !list.Contains( current ) )
			list.Add( current );

		_suppress = true;
		try
		{
			TokenTypeCombo.ItemsSource = list;
			TokenTypeCombo.SelectedItem = current;
			SyncTerrainComboForScenario();
		}
		finally
		{
			_suppress = false;
		}
	}

	void SyncTerrainComboForScenario()
	{
		if ( _scenario == null || _interaction == null )
			return;
		var terrains = ScenarioCollectionAvailability.GetSelectableTerrainTypes( _scenario ).ToList();
		if ( terrains.Count == 0 )
			terrains.Add( TerrainType.Boulder );
		var current = _interaction.terrainType;
		if ( !terrains.Contains( current ) )
			terrains.Insert( 0, current );
		_suppress = true;
		try
		{
			TerrainTypeCombo.ItemsSource = terrains;
			TerrainTypeCombo.SelectedItem = current;
		}
		finally
		{
			_suppress = false;
		}
	}

	/// <summary>Re-applies scenario collection rules to token type / terrain lists (called from <see cref="BaseEventDialogWindow"/>).</summary>
	public void ApplyScenarioCollectionAvailability( Scenario scenario )
	{
		_scenario = scenario;
		if ( _interaction == null )
			return;
		PopulateTokenTypeCombo();
		_suppress = true;
		try
		{
			PersonTypeCombo.SelectedItem = _interaction.personType == PersonType.None ? PersonType.Human : _interaction.personType;
			SyncTerrainComboForScenario();
			UpdateDetailVisibility();
		}
		finally
		{
			_suppress = false;
		}
	}

	public void Hydrate()
	{
		if ( _interaction == null )
			return;
		_suppress = true;
		try
		{
			IsTokenInteractionCheckBox.IsChecked = _interaction.isTokenInteraction;
			TokenInteractionTextBox.Text = _interaction.tokenInteractionText ?? "";
			PopulateTokenTypeCombo();
			PersonTypeCombo.SelectedItem = _interaction.personType == PersonType.None ? PersonType.Human : _interaction.personType;
			SyncTerrainComboForScenario();
			UpdateDetailVisibility();
			if ( _interaction is TextInteraction )
			{
				TokenPersistentRow.IsVisible = false;
			}
			else if ( _interaction is PersistentInteractionBase p )
			{
				TokenPersistentRow.IsVisible = true;
				TokenPersistentCheckBox.IsChecked = p.isPersistent;
				EditPersistentFromTokenButton.IsEnabled = p.isPersistent;
			}
			else
			{
				TokenPersistentRow.IsVisible = false;
			}
		}
		finally
		{
			_suppress = false;
		}
	}

	public void Commit()
	{
		if ( _interaction == null )
			return;
		_interaction.isTokenInteraction = IsTokenInteractionCheckBox.IsChecked == true;
		_interaction.tokenInteractionText = TokenInteractionTextBox.Text ?? "";
		if ( !_interaction.isTokenInteraction )
		{
			// Dialog persistent flag lives on the custom tab, not the token row.
			if ( _interaction is PersistentInteractionBase p && _interaction is not DialogInteraction )
				p.isPersistent = false;
			return;
		}

		if ( TokenTypeCombo.SelectedItem is TokenType tt )
			_interaction.tokenType = tt;
		if ( PersonTypeRow.IsVisible && PersonTypeCombo.SelectedItem is PersonType pt )
			_interaction.personType = pt;
		if ( TerrainTypeRow.IsVisible && TerrainTypeCombo.SelectedItem is TerrainType tr )
			_interaction.terrainType = tr;

		if ( _interaction is PersistentInteractionBase p2 && _interaction is not TextInteraction )
			p2.isPersistent = TokenPersistentCheckBox.IsChecked == true;
	}

	void UpdateDetailVisibility()
	{
		var on = IsTokenInteractionCheckBox.IsChecked == true;
		TokenDetailsPanel.IsVisible = on;
		if ( !on )
		{
			PersonTypeRow.IsVisible = false;
			TerrainTypeRow.IsVisible = false;
			return;
		}

		var tt = TokenTypeCombo.SelectedItem as TokenType? ?? _interaction?.tokenType;
		PersonTypeRow.IsVisible = tt == TokenType.Person;
		TerrainTypeRow.IsVisible = tt == TokenType.Terrain;
	}

	void IsTokenInteractionCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _interaction == null )
			return;
		_interaction.isTokenInteraction = IsTokenInteractionCheckBox.IsChecked == true;
		if ( _interaction.isTokenInteraction )
			_interaction.triggerName = "None";
		UpdateDetailVisibility();
	}

	void TokenPersistentCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _interaction is TextInteraction )
			return;
		if ( _interaction is PersistentInteractionBase p )
			p.isPersistent = TokenPersistentCheckBox.IsChecked == true;
		EditPersistentFromTokenButton.IsEnabled = TokenPersistentCheckBox.IsChecked == true;
	}

	void TokenTypeCombo_OnSelectionChanged( object? sender, SelectionChangedEventArgs e )
	{
		if ( _suppress || _scenario == null || _interaction == null )
			return;
		if ( TokenTypeCombo.SelectedItem is TokenType selected )
		{
			_interaction.tokenType = selected;
			ScenarioCollectionAvailability.CoerceInteractionTokenForScenario( _interaction, _scenario, _scenario.scenarioTypeJourney );
			if ( TokenTypeCombo.SelectedItem is TokenType coerced && coerced != selected )
			{
				_suppress = true;
				try
				{
					TokenTypeCombo.SelectedItem = coerced;
				}
				finally
				{
					_suppress = false;
				}
			}
		}
		if ( TokenTypeCombo.SelectedItem is TokenType.Terrain )
			SyncTerrainComboForScenario();
		UpdateDetailVisibility();
	}

	async void TokenHelpButton_OnClick( object? sender, RoutedEventArgs e )
	{
		var w = TopLevel.GetTopLevel( this ) as Window;
		if ( w != null )
			await HelpWindow.ShowDialogAsync( w, HelpType.Token, 1 );
	}

	async void EditPersistentFromToken_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction is TextInteraction )
			return;
		if ( _interaction is not PersistentInteractionBase p )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var dlg = new StoryTextEditWindow( p.persistentText );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
			p.persistentText = dlg.StoryText;
	}
}
