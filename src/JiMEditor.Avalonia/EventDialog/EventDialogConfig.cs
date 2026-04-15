// Ported pattern: JiMEditor interaction editors — shared shell config (2026-04-13).
using System;
using Avalonia.Controls;

namespace JiMEditor.Avalonia.EventDialog;

/// <summary>Per-event tuning for the shared General Properties stack (and shell chrome).</summary>
public sealed class EventDialogConfig
{
	public string CustomTabTitle { get; init; } = "Custom";

	/// <summary>WPF default name that cannot be kept when saving (e.g. Dialog <c>"New Dialog Event"</c>).</summary>
	public string? ForbiddenDefaultEventName { get; init; }

	public EventSectionVis ThreatCallout { get; init; } = EventSectionVis.Collapsed;
	public EventSectionVis EventNameGroup { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Instructions { get; init; } = EventSectionVis.Collapsed;
	public EventSectionVis Rewards { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Token { get; init; } = EventSectionVis.Visible;
	public EventSectionVis Triggers { get; init; } = EventSectionVis.Visible;
	public EventSectionVis EventText { get; init; } = EventSectionVis.Visible;
	public EventSectionVis FlavorText { get; init; } = EventSectionVis.Visible;
	public bool ShowCustomTab { get; init; } = true;

	/// <summary>When flavor is visible but read-only (no edit button), as for Dialog events.</summary>
	public bool FlavorReadOnly { get; init; }
	public string FlavorBodyText { get; init; } =
		"Flavor text shown when this Event is triggered by interacting with its Token.";
	public string EventBodyText { get; init; } =
		"Use the Event Text to describe the actual Event that has been activated. Example: Describe an Enemy Threat, present a Test, describe a Decision, etc.";
	public string TriggerHeadingText { get; init; } = "Triggered By (Optional):";
	public string TriggerBodyText { get; init; } =
		"Multiple Events can be 'Triggered By' the same Trigger. When this happens, only ONE of the Events will be randomly chosen to fire.";

	/// <summary>Optional callout/control inserted between Event Name and Rewards on General Properties.</summary>
	public Func<Control>? InstructionsFactory { get; init; }
	/// <summary>Optional replacement shown where the Token section normally appears.</summary>
	public Func<Control>? TokenSectionOverrideFactory { get; init; }
	/// <summary>Optional replacement shown where the Trigger section normally appears.</summary>
	public Func<Control>? TriggerSectionOverrideFactory { get; init; }
}
