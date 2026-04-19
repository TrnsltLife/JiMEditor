// Ported behavior: JiMEditor uses trailing GRP# on event names for random token groups (see Help / docs).
// Docs show a space before GRP; names may also end with GRP# with no space — treat both consistently.
using System.Text.RegularExpressions;

namespace JiME;

public static class InteractionGroupNaming
{
	public static readonly Regex GroupSuffixRegex = new( @"\s*(GRP\d+)$", RegexOptions.Compiled );

	public static bool HasGroupSuffix( string? dataName ) =>
		dataName != null && GroupSuffixRegex.IsMatch( dataName );

	/// <summary>Returns e.g. "GRP1", or null if the name has no group suffix.</summary>
	public static string? TryGetGroupKey( string? dataName )
	{
		if ( string.IsNullOrEmpty( dataName ) )
			return null;
		var m = GroupSuffixRegex.Match( dataName );
		return m.Success ? m.Groups[1].Value : null;
	}
}
