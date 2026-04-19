// Ported from: JiMEditor/Views/Interactions/TestInteractionWindow.xaml.cs (2026-04-19) — Avalonia + BaseEventDialogWindow.
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;

namespace JiMEditor.Avalonia.Views.Events;

public sealed class TestInteractionWindow : BaseEventDialogWindow
{
	readonly TestInteraction _test;
	TestStatCustomTab? _tab;

	public TestInteractionWindow( Scenario scenario )
		: this( scenario, new TestInteraction( "New Stat Test" ), showCancelButton: true )
	{
	}

	public TestInteractionWindow( Scenario scenario, TestInteraction interaction, bool showCancelButton = false )
		: base( scenario, interaction, EventDialogConfigFactory.StatTest(), showCancelButton )
	{
		_test = interaction;
		Title = "Stat Test Editor";
	}

	public TestInteraction interaction => _test;

	protected override Control CreateCustomTab()
	{
		_tab = new TestStatCustomTab();
		_tab.Attach( Scenario, _test );
		return _tab;
	}

	protected override void CommitCustomTabToModel() => _tab?.Commit();
}
