using JiMEditor.Avalonia.EventDialog;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class EventDialogConfigFactoryTests
{
	[Fact]
	public void Dialog_preset_has_expected_tab_title_and_forbidden_name()
	{
		var c = EventDialogConfigFactory.Dialog();
		Assert.Equal( "Dialog", c.CustomTabTitle );
		Assert.Equal( "New Dialog Event", c.ForbiddenDefaultEventName );
		Assert.True( c.FlavorReadOnly );
	}
}
