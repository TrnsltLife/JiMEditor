// Ported from: JiMEditor/UserControls/TokenTypeSelector.xaml.cs — Avalonia subset (2026-04-13).
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventTokenSection : UserControl
{
	bool _suppress;
	bool _isThreatTriggered;
	Scenario? _scenario;
	InteractionBase? _interaction;
	PropertyChangedEventHandler? _interactionPropertyHandler;

	static readonly TokenType[] s_selectableTokenTypes =
	{
		TokenType.Search, TokenType.Person, TokenType.Threat, TokenType.Darkness,
		TokenType.DifficultGround, TokenType.Fortified, TokenType.Terrain
	};

	public EventTokenSection()
	{
		InitializeComponent();
		PersonTypeCombo.ItemsSource = new[] { PersonType.Human, PersonType.Elf, PersonType.Hobbit, PersonType.Dwarf };
		TerrainTypeCombo.ItemsSource = Enum.GetValues<TerrainType>().Where( t => t != TerrainType.None ).ToArray();
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
		var list = new List<TokenType>();
		foreach ( var tt in s_selectableTokenTypes )
		{
			if ( tt is TokenType.DifficultGround or TokenType.Fortified )
			{
				var enabled = _scenario?.collectionObserver.Any( c =>
					( tt == TokenType.DifficultGround && c.DifficultGround ) ||
					( tt == TokenType.Fortified && c.Fortified ) ) == true;
				if ( !enabled )
					continue;
			}

			if ( tt == TokenType.Terrain && _scenario is { scenarioTypeJourney: true } )
				continue;

			list.Add( tt );
		}

		_suppress = true;
		try
		{
			TokenTypeCombo.ItemsSource = list;
			if ( _interaction != null && list.Contains( _interaction.tokenType ) )
				TokenTypeCombo.SelectedItem = _interaction.tokenType;
			else if ( list.Count > 0 )
				TokenTypeCombo.SelectedItem = list.Contains( TokenType.Search ) ? TokenType.Search : list[0];
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
			TerrainTypeCombo.SelectedItem = _interaction.terrainType == TerrainType.None ? TerrainType.Boulder : _interaction.terrainType;
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
			if ( _interaction is PersistentInteractionBase p )
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
		if ( _suppress )
			return;
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
