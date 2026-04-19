// Event reference dropdowns: exclude the scenario Start interaction (2026-04-19).
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public static class EventDialogInteractionLists
{
	/// <summary>
	/// The Starting Position event must not appear in event pickers inside event editors; it remains in the main scenario Events list only.
	/// </summary>
	public static bool IncludeInReferenceDropdown( IInteraction interaction ) =>
		interaction.interactionType != InteractionType.Start;
}
