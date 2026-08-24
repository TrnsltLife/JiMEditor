// Ported from: JiMEditor/Common/Commands.cs KeyGestures (2026-08-23).
using System;
using Avalonia.Input;

namespace JiMEditor.Avalonia;

/// <summary>
/// OS-primary modifier (Control on Windows/Linux, Command/Meta on macOS) plus Alt gestures from WPF JiMEditor.
/// </summary>
public static class EditorHotKeys
{
	public static KeyModifiers PrimaryModifier =>
		OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

	public static string PrimaryModifierLabel =>
		OperatingSystem.IsMacOS() ? "⌘" : "Ctrl";

	public static bool HasPrimaryModifier( KeyModifiers modifiers ) =>
		modifiers.HasFlag( PrimaryModifier );

	public static bool HasPrimaryModifier( KeyEventArgs e ) =>
		HasPrimaryModifier( e.KeyModifiers );

	public static bool Matches( KeyEventArgs e, Key key, KeyModifiers extra = KeyModifiers.None ) =>
		Matches( e.Key, e.KeyModifiers, key, extra );

	public static bool Matches( Key key, KeyModifiers modifiers, Key expectedKey, KeyModifiers extra = KeyModifiers.None )
	{
		if ( key != expectedKey )
			return false;
		var expected = PrimaryModifier | extra;
		var relevant = modifiers & ( KeyModifiers.Control | KeyModifiers.Meta | KeyModifiers.Alt | KeyModifiers.Shift );
		return relevant == expected;
	}

	public static bool MatchesAlt( KeyEventArgs e, Key key ) =>
		MatchesAlt( e.Key, e.KeyModifiers, key );

	public static bool MatchesAlt( Key key, KeyModifiers modifiers, Key expectedKey )
	{
		if ( key != expectedKey )
			return false;
		var relevant = modifiers & ( KeyModifiers.Control | KeyModifiers.Meta | KeyModifiers.Alt | KeyModifiers.Shift );
		return relevant == KeyModifiers.Alt;
	}
}
