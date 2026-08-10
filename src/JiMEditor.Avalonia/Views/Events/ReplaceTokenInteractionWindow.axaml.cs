// Ported from: JiMEditor/Views/Interactions/ReplaceTokenInteractionWindow.xaml.cs (2026-04-17) — Avalonia + BaseEventDialogWindow.
using System;
using System.Linq;
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class ReplaceTokenInteractionWindow : BaseEventDialogWindow
{
	readonly ReplaceTokenInteraction _replace;
	ReplaceTokenCustomTab? _tab;

	public ReplaceTokenInteractionWindow( Scenario scenario )
		: this( scenario, new ReplaceTokenInteraction( "New Replace Token Event" ), showCancelButton: true )
	{
	}

	public ReplaceTokenInteractionWindow( Scenario scenario, ReplaceTokenInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.ReplaceToken(), showCancelButton )
	{
		_replace = interaction;
		Title = "Replace Token Event Editor";
	}

	public ReplaceTokenInteraction interaction => _replace;

	protected override Control CreateCustomTab()
	{
		_tab = new ReplaceTokenCustomTab();
		_tab.Attach( Scenario, _replace );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();

	protected override bool TryValidateForOk()
	{
		if ( !base.TryValidateForOk() )
			return false;

		if ( _replace.eventToReplace == _replace.dataName
		     || _replace.replaceWithEvent == _replace.dataName )
		{
			MessageBox.Show(
				"This Event can't replace or be replaced with itself.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		if ( _replace.eventToReplace == "None" || _replace.replaceWithEvent == "None" )
		{
			MessageBox.Show(
				"The target and source Events can't be set to None.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		if ( _replace.eventToReplace == _replace.replaceWithEvent )
		{
			MessageBox.Show(
				"The target and source replacement Events can't be the same.",
				"Data Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
			return false;
		}

		var source = Scenario.interactionObserver.FirstOrDefault( x => x.dataName == _replace.replaceWithEvent );
		_replace.replaceWithGUID = source?.GUID ?? Guid.Empty;
		return true;
	}
}
