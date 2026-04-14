// Ported pattern: JiMEditor interaction editors — config presets (2026-04-13).
namespace JiMEditor.Avalonia.EventDialog;

public static class EventDialogConfigFactory
{
	public static EventDialogConfig Dialog() =>
		new()
		{
			CustomTabTitle = "Dialog",
			ForbiddenDefaultEventName = "New Dialog Event",
			ThreatCallout = EventSectionVis.Visible,
			EventNameGroup = EventSectionVis.Visible,
			Rewards = EventSectionVis.Visible,
			Token = EventSectionVis.Visible,
			Triggers = EventSectionVis.Visible,
			EventText = EventSectionVis.Visible,
			FlavorText = EventSectionVis.Visible,
			FlavorReadOnly = true
		};
}
