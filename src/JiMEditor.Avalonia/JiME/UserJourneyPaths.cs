// Avalonia port: isolate user data from WPF JiMEditor (Documents/Your Journey).
using System;
using System.IO;

namespace JiME;

/// <summary>
/// Base folder under <see cref="Environment.SpecialFolder.MyDocuments"/> for this build.
/// </summary>
public static class UserJourneyPaths
{
	public const string DocumentFolderName = "Your Journey Avalonia";

	static string? _testBasePathOverride;

	/// <summary>Redirect document storage (parity UI tests).</summary>
	public static void SetTestBasePath( string path ) => _testBasePathOverride = path;

	public static void ClearTestBasePath() => _testBasePathOverride = null;

	public static string GetBasePath() =>
		_testBasePathOverride
		?? Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), DocumentFolderName );

	/// <summary>Window title prefix so Avalonia runs are distinguishable from WPF.</summary>
	public const string EditorWindowTitlePrefix = "Your Journey Avalonia";

	/// <summary>User-facing relative path hint; separator matches the current OS.</summary>
	public static string DocumentsRelativePathHint =>
		Path.Combine( "Documents", DocumentFolderName );

	public static string CampaignScenarioRemovalFootnote =>
		"Removing a Scenario from the Campaign DOES NOT DELETE ITS FILE. The Scenario's file will be moved into the base Scenario folder ("
		+ DocumentsRelativePathHint
		+ "). Hold CONTROL while clicking the X button to PERMANENTLY DELETE the file (you will be asked to confirm).";
}
