using System.Linq;
using JiMEditor.Avalonia.Views.Events.EventDialogs;
using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class EventDialogTriggerSortTests
{
	[Fact]
	public void Sort_orders_nonempty_before_empty_then_by_name()
	{
		var triggers = new[]
		{
			new Trigger( "Zeta" ) { isEmpty = true },
			new Trigger( "Alpha" ) { isEmpty = false },
			new Trigger( "Beta" ) { isEmpty = true }
		};

		var sorted = EventDialogTriggerSort.Sort( triggers ).Select( t => t.dataName ).ToArray();

		Assert.Equal( new[] { "Alpha", "Beta", "Zeta" }, sorted );
	}
}
