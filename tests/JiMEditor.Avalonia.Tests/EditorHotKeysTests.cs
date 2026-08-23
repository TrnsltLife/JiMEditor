using Avalonia.Input;
using JiMEditor.Avalonia;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class EditorHotKeysTests
{
	[Fact]
	public void Matches_PrimaryPlusS_RequiresOsPrimaryModifierOnly()
	{
		var primary = EditorHotKeys.PrimaryModifier;
		var other = primary == KeyModifiers.Meta ? KeyModifiers.Control : KeyModifiers.Meta;
		Assert.True( EditorHotKeys.Matches( Key.S, primary, Key.S ) );
		Assert.False( EditorHotKeys.Matches( Key.S, other, Key.S ) );
		Assert.False( EditorHotKeys.Matches( Key.S, primary | KeyModifiers.Shift, Key.S ) );
	}

	[Fact]
	public void Matches_SaveAs_IsPrimaryPlusAltS()
	{
		var chord = EditorHotKeys.PrimaryModifier | KeyModifiers.Alt;
		Assert.True( EditorHotKeys.Matches( Key.S, chord, Key.S, KeyModifiers.Alt ) );
		Assert.False( EditorHotKeys.Matches( Key.S, EditorHotKeys.PrimaryModifier, Key.S, KeyModifiers.Alt ) );
	}

	[Fact]
	public void MatchesAlt_IgnoresPrimaryModifierChords()
	{
		Assert.True( EditorHotKeys.MatchesAlt( Key.E, KeyModifiers.Alt, Key.E ) );
		Assert.False( EditorHotKeys.MatchesAlt( Key.E, EditorHotKeys.PrimaryModifier | KeyModifiers.Alt, Key.E ) );
		Assert.False( EditorHotKeys.MatchesAlt( Key.E, KeyModifiers.None, Key.E ) );
	}
}
