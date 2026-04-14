// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml — name/group row (2026-04-13).
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventNameAndGroupSection : UserControl
{
	static readonly Regex s_groupSuffix = new( @"\sGRP\d+$", RegexOptions.Compiled );

	InteractionBase? _interaction;

	public EventNameAndGroupSection()
	{
		InitializeComponent();
	}

	public void Attach( InteractionBase interaction )
	{
		_interaction = interaction;
	}

	public void Hydrate()
	{
		if ( _interaction == null )
			return;
		NameTextBox.Text = _interaction.dataName ?? "";
		IsReusableCheckBox.IsChecked = _interaction.isReusable;
		UpdateGroupUi();
	}

	public void CommitNameToModel()
	{
		if ( _interaction == null )
			return;
		_interaction.dataName = NameTextBox.Text ?? "";
		_interaction.isReusable = IsReusableCheckBox.IsChecked == true;
	}

	void NameTextBox_OnTextChanged( object? sender, TextChangedEventArgs e )
	{
		if ( _interaction == null )
			return;
		_interaction.dataName = NameTextBox.Text ?? "";
		UpdateGroupUi();
	}

	void UpdateGroupUi()
	{
		var name = _interaction?.dataName ?? "";
		var matches = s_groupSuffix.Matches( name );
		if ( matches.Count > 0 )
		{
			GroupInfoText.Text = "This Event is in the following group: " + matches[0].Value.Trim();
			IsReusableCheckBox.IsVisible = true;
		}
		else
		{
			GroupInfoText.Text = "This Event is in the following group: None";
			IsReusableCheckBox.IsVisible = false;
		}
	}

	async void GroupHelpButton_OnClick( object? sender, RoutedEventArgs e )
	{
		var w = TopLevel.GetTopLevel( this ) as Window;
		if ( w != null )
			await HelpWindow.ShowDialogAsync( w, HelpType.Grouping, 0 );
	}
}
