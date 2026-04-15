using Avalonia.Controls;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public interface IEventCustomTab
{
	void Hydrate();
	void Detach();
	Control AsControl();
}
