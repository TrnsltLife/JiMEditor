// Ported pattern: JiMEditor interaction editors — section chrome helpers (2026-04-13).
using Avalonia.Controls;
using JiMEditor.Avalonia.EventDialog;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public static class EventDialogSectionHelper
{
	public static void ApplyVisibility( Control root, EventSectionVis vis )
	{
		switch ( vis )
		{
			case EventSectionVis.Visible:
				root.IsVisible = true;
				root.IsEnabled = true;
				break;
			case EventSectionVis.Collapsed:
				root.IsVisible = false;
				break;
			case EventSectionVis.Disabled:
				root.IsVisible = true;
				root.IsEnabled = false;
				break;
		}
	}
}
