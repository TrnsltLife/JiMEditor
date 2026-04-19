// Ported from: JiMEditor/UserControls/TokenTypeSelector.xaml — persistent token UI for Text events (2026-04-17).
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class TextEventPersistentTab : UserControl, IEventCustomTab
{
	const string SkipInteractionPseudoHtml =
		"""
		Skip the <font="Icon">I</font> Interaction button and the Flavor Text and Event Text.
		Just show the Persistent Text + Continue button when the token is clicked.
		""";

	bool _suppress;
	TextInteraction? _text;
	PropertyChangedEventHandler? _handler;

	public TextEventPersistentTab()
	{
		InitializeComponent();
		SkipInteractionLabel.PseudoHtml = SkipInteractionPseudoHtml;
	}

	public void Attach( Scenario _, TextInteraction text )
	{
		DetachHandler();
		_text = text;
		_handler = OnTextPropertyChanged;
		_text.PropertyChanged += _handler;
	}

	public void Detach() => DetachHandler();

	void DetachHandler()
	{
		if ( _text != null && _handler != null )
			_text.PropertyChanged -= _handler;
		_handler = null;
		_text = null;
	}

	Control IEventCustomTab.AsControl() => this;

	void OnTextPropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( !string.IsNullOrEmpty( e.PropertyName )
		     && e.PropertyName != nameof( InteractionBase.isTokenInteraction )
		     && e.PropertyName != nameof( PersistentInteractionBase.isPersistent )
		     && e.PropertyName != nameof( PersistentInteractionBase.hasActivated ) )
			return;
		_suppress = true;
		try
		{
			if ( _text != null )
			{
				TextPersistentCheckBox.IsChecked = _text.isPersistent;
				SkipInteractionCheckBox.IsChecked = _text.hasActivated;
			}
		}
		finally
		{
			_suppress = false;
		}

		SyncControlStates();
	}

	public void Hydrate()
	{
		if ( _text == null )
			return;
		_suppress = true;
		try
		{
			TextPersistentCheckBox.IsChecked = _text.isPersistent;
			SkipInteractionCheckBox.IsChecked = _text.hasActivated;
			SyncControlStates();
		}
		finally
		{
			_suppress = false;
		}
	}

	void SyncControlStates()
	{
		var tokenOn = _text?.isTokenInteraction == true;
		NotTokenHintBorder.IsVisible = !tokenOn;
		TextPersistentCheckBox.IsEnabled = tokenOn;
		var persistentOn = tokenOn && TextPersistentCheckBox.IsChecked == true;
		EditPersistentTextButton.IsEnabled = persistentOn;
		SkipInteractionCheckBox.IsVisible = persistentOn;
		if ( persistentOn )
			SkipInteractionCheckBox.IsEnabled = true;
	}

	internal void Commit()
	{
		if ( _text == null || !_text.isTokenInteraction )
			return;
		_text.isPersistent = TextPersistentCheckBox.IsChecked == true;
		_text.hasActivated = _text.isPersistent && SkipInteractionCheckBox.IsChecked == true;
	}

	void TextPersistentCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _text == null )
			return;
		_text.isPersistent = TextPersistentCheckBox.IsChecked == true;
		if ( !_text.isPersistent )
			_text.hasActivated = false;
		SyncControlStates();
		if ( !_text.isPersistent )
		{
			_suppress = true;
			try
			{
				SkipInteractionCheckBox.IsChecked = false;
			}
			finally
			{
				_suppress = false;
			}
		}
	}

	void SkipInteractionCheckBox_OnIsCheckedChanged( object? sender, RoutedEventArgs e )
	{
		if ( _suppress || _text == null )
			return;
		if ( _text.isTokenInteraction && _text.isPersistent )
			_text.hasActivated = SkipInteractionCheckBox.IsChecked == true;
	}

	async void EditPersistentText_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _text == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var dlg = new StoryTextEditWindow( _text.persistentText );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
			_text.persistentText = dlg.StoryText;
	}
}
