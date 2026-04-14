// Ported pattern: JiMEditor interaction editors — shared shell config (2026-04-13).
namespace JiMEditor.Avalonia.EventDialog;

/// <summary>Per-event tuning for the shared General Properties stack (and shell chrome).</summary>
public sealed class EventDialogConfig
{
	public string CustomTabTitle { get; init; } = "Custom";

	/// <summary>WPF default name that cannot be kept when saving (e.g. Dialog <c>"New Dialog Event"</c>).</summary>
	public string? ForbiddenDefaultEventName { get; init; }

	public EventSectionVis ThreatCallout { get; init; } = EventSectionVis.Collapsed;
	public EventSectionVis EventNameGroup { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Rewards { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Token { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Triggers { get; init; } = EventSectionVis.Visible;
	public EventSectionVis EventText { get; init; } = EventSectionVis.Visible;
	public EventSectionVis FlavorText { get; init; } = EventSectionVis.Visible;

	/// <summary>When flavor is visible but read-only (no edit button), as for Dialog events.</summary>
	public bool FlavorReadOnly { get; init; }
}
