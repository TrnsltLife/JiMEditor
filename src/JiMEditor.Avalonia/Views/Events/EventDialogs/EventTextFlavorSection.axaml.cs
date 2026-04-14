// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml — event/flavor (2026-04-13).
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventTextFlavorSection : UserControl
{
	InteractionBase? _interaction;
	bool _flavorReadOnly;

	public EventTextFlavorSection()
	{
		InitializeComponent();
	}

	public void Attach( InteractionBase interaction, bool flavorReadOnly, string flavorBodyText )
	{
		_interaction = interaction;
		_flavorReadOnly = flavorReadOnly;
		FlavorBodyText.Text = flavorBodyText;
		EditFlavorTextButton.IsVisible = !flavorReadOnly;
	}

	async void EditEventText_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction == null )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var page0 = _interaction.eventBookData.pages.ElementAtOrDefault( 0 ) ?? "";
		var dlg = new StoryTextEditWindow( page0 );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
		{
			if ( _interaction.eventBookData.pages.Count == 0 )
				_interaction.eventBookData.pages.Add( dlg.StoryText );
			else
				_interaction.eventBookData.pages[0] = dlg.StoryText;
		}
	}

	async void EditFlavorText_OnClick( object? sender, RoutedEventArgs e )
	{
		if ( _interaction == null || _flavorReadOnly )
			return;
		var owner = TopLevel.GetTopLevel( this ) as Window;
		if ( owner == null )
			return;
		var page0 = _interaction.textBookData.pages.ElementAtOrDefault( 0 ) ?? "";
		var dlg = new StoryTextEditWindow( page0 );
		await dlg.ShowDialog( owner );
		if ( dlg.Accepted )
		{
			if ( _interaction.textBookData.pages.Count == 0 )
				_interaction.textBookData.pages.Add( dlg.StoryText );
			else
				_interaction.textBookData.pages[0] = dlg.StoryText;
		}
	}
}
