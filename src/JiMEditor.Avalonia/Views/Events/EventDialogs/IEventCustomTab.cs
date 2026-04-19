using Avalonia.Controls;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public interface IEventCustomTab
{
	void Hydrate();
	void Detach();
	Control AsControl();

	/// <summary>Optional collection-based UI/model sync after hydrate (items, titles, threat pool, etc.).</summary>
	void ApplyScenarioCollectionAvailability( Scenario scenario ) { }
}
