// Ported from: JiMEditor/Common/Common.cs — version strings only; Init is a no-op until editor loads tiles (2026-04-04)

namespace JiME;

public static class Utils
{
	/// <summary>Updated when the file format changes (companion app "Engine Version").</summary>
	public static string formatVersion = "1.40";
	public static string appVersion = "0.40";

	public static void Init()
	{
		// TODO-Avalonia-Reenable: port tile dictionaries and grid snaps from WPF Utils.Init when MainWindow exists.
	}
}
