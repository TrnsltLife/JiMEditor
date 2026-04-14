// Ported pattern: WPF CollectionViewSource sort on triggersObserver (2026-04-13).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public static class EventDialogTriggerSort
{
	public static IEnumerable<Trigger> Sort( IEnumerable raw ) =>
		raw.Cast<Trigger>()
			// Non-empty triggers first, then by name (WPF combo: usable triggers before the empty "None" row).
			.OrderBy( t => t.isEmpty )
			.ThenBy( t => t.dataName, System.StringComparer.OrdinalIgnoreCase );
}
