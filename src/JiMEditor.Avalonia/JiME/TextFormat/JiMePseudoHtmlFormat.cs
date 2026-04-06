// Ported from: JiMEditor/UserControls/RichTextBoxIconEditor.xaml.cs — pseudo-HTML parse/serialize without FlowDocument (2026-04-05).
using System;
using System.Collections.Generic;
using System.Text;

namespace JiME.TextFormat;

/// <summary>
/// JiME "simple HTML" stored in JSON: <c>&lt;font="Icon"&gt;…&lt;/font&gt;</c>, <c>&lt;b&gt;</c>, <c>&lt;i&gt;</c>, paragraphs as <c>\r\n\r\n</c>.
/// Matches WPF regex pipeline order for nesting (icon + bold + italic).
/// Parse repairs mis-nested closes, drops non-Icon <c>&lt;font…&gt;</c> opens, and treats unclosed tags as spanning to paragraph end.
/// Serialize always emits canonical nesting: font outside, then b, then i.
/// </summary>
public static class JiMePseudoHtmlFormat
{
	public const string ParagraphSeparator = "\r\n\r\n";

	public const string IconFontFamilyName = "LoTR JiME Icons";

	const string FontIconOpen = "<font=\"Icon\">";

	/// <summary>Parse stored pseudo-HTML into a document. Null or empty → single empty paragraph.</summary>
	public static JiMeTextDocument Parse( string? html )
	{
		if ( string.IsNullOrEmpty( html ) )
			return JiMeTextDocument.Empty();

		var doc = new JiMeTextDocument();
		var parts = html.Split( ParagraphSeparator, StringSplitOptions.None );
		foreach ( var part in parts )
			doc.Paragraphs.Add( ParseParagraph( part ) );
		return doc;
	}

	/// <summary>Serialize to the exact pseudo-HTML string stored in .jime / consumed by Unity.</summary>
	public static string Serialize( JiMeTextDocument? doc )
	{
		if ( doc == null || doc.Paragraphs.Count == 0 )
			return "";

		var sb = new StringBuilder();
		for ( var p = 0; p < doc.Paragraphs.Count; p++ )
		{
			if ( p > 0 )
				sb.Append( ParagraphSeparator );
			SerializeParagraph( sb, doc.Paragraphs[p] );
		}

		return sb.ToString();
	}

	/// <summary>Parse → serialize with repairs (tag order, interleaving, unterminated tags, Icon-only font tags).</summary>
	public static string NormalizePseudoHtml( string? html ) =>
		Serialize( Parse( html ) );

	static JiMeTextParagraph ParseParagraph( string paragraph )
	{
		var result = new JiMeTextParagraph();
		var stack = new List<FmtTag>();
		var lit = new StringBuilder();
		var i = 0;
		var s = paragraph;

		void Flush()
		{
			if ( lit.Length == 0 )
				return;
			var (icon, bold, italic) = AggregateFormat( stack );
			result.Runs.Add( new JiMeTextRun
			{
				Text = lit.ToString(),
				IsIconFont = icon,
				Bold = bold,
				Italic = italic
			} );
			lit.Clear();
		}

		while ( i < s.Length )
		{
			if ( s[i] != '<' )
			{
				lit.Append( s[i++] );
				continue;
			}

			Flush();

			if ( TryConsume( s, ref i, FontIconOpen ) )
			{
				stack.Add( FmtTag.IconFont );
				continue;
			}

			if ( IsNonIconFontOpen( s, i ) )
			{
				SkipThroughClosingAngle( s, ref i );
				continue;
			}

			if ( TryConsume( s, ref i, "</font>" ) )
			{
				CloseTagRepairing( stack, FmtTag.IconFont );
				continue;
			}

			if ( TryConsume( s, ref i, "<b>" ) )
			{
				stack.Add( FmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, "</b>" ) )
			{
				CloseTagRepairing( stack, FmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, "<i>" ) )
			{
				stack.Add( FmtTag.Italic );
				continue;
			}

			if ( TryConsume( s, ref i, "</i>" ) )
			{
				CloseTagRepairing( stack, FmtTag.Italic );
				continue;
			}

			// Unknown markup: keep literal '<'
			lit.Append( s[i++] );
		}

		Flush();
		MergeAdjacentSameFormat( result.Runs );
		return result;
	}

	enum FmtTag
	{
		IconFont,
		Bold,
		Italic
	}

	static (bool Icon, bool Bold, bool Italic) AggregateFormat( List<FmtTag> stack )
	{
		var icon = false;
		var bold = false;
		var italic = false;
		for ( var k = 0; k < stack.Count; k++ )
		{
			switch ( stack[k] )
			{
				case FmtTag.IconFont:
					icon = true;
					break;
				case FmtTag.Bold:
					bold = true;
					break;
				case FmtTag.Italic:
					italic = true;
					break;
			}
		}

		return ( icon, bold, italic );
	}

	/// <summary>Pop inner tags until <paramref name="expected"/> is on top, then pop it. Stray closes are ignored.</summary>
	static void CloseTagRepairing( List<FmtTag> stack, FmtTag expected )
	{
		if ( stack.Count == 0 )
			return;
		var found = false;
		for ( var k = stack.Count - 1; k >= 0; k-- )
		{
			if ( stack[k] == expected )
			{
				found = true;
				break;
			}
		}

		if ( !found )
			return;
		while ( stack.Count > 0 && stack[^1] != expected )
			stack.RemoveAt( stack.Count - 1 );
		if ( stack.Count > 0 && stack[^1] == expected )
			stack.RemoveAt( stack.Count - 1 );
	}

	static bool IsNonIconFontOpen( string s, int i )
	{
		if ( i >= s.Length || s[i] != '<' )
			return false;
		const string prefix = "<font";
		if ( i + prefix.Length > s.Length )
			return false;
		if ( string.CompareOrdinal( s, i, prefix, 0, prefix.Length ) != 0 )
			return false;
		// Exact Icon open is handled before this helper is called.
		if ( i + FontIconOpen.Length <= s.Length && string.CompareOrdinal( s, i, FontIconOpen, 0, FontIconOpen.Length ) == 0 )
			return false;
		return true;
	}

	static void SkipThroughClosingAngle( string s, ref int i )
	{
		while ( i < s.Length && s[i] != '>' )
			i++;
		if ( i < s.Length && s[i] == '>' )
			i++;
	}

	static bool TryConsume( string s, ref int i, string token )
	{
		if ( i + token.Length > s.Length )
			return false;
		if ( string.CompareOrdinal( s, i, token, 0, token.Length ) != 0 )
			return false;
		i += token.Length;
		return true;
	}

	static void SerializeParagraph( StringBuilder sb, JiMeTextParagraph paragraph )
	{
		MergeAdjacentSameFormat( paragraph.Runs );
		foreach ( var run in paragraph.Runs )
			SerializeRun( sb, run );
	}

	static void SerializeRun( StringBuilder sb, JiMeTextRun run )
	{
		var t = run.Text ?? "";
		if ( t.Length == 0 )
			return;

		if ( run.IsIconFont )
		{
			if ( run.Bold && run.Italic )
				sb.Append( "<font=\"Icon\"><b><i>" ).Append( t ).Append( "</i></b></font>" );
			else if ( run.Bold )
				sb.Append( "<font=\"Icon\"><b>" ).Append( t ).Append( "</b></font>" );
			else if ( run.Italic )
				sb.Append( "<font=\"Icon\"><i>" ).Append( t ).Append( "</i></font>" );
			else
				sb.Append( "<font=\"Icon\">" ).Append( t ).Append( "</font>" );
		}
		else
		{
			if ( run.Bold && run.Italic )
				sb.Append( "<b><i>" ).Append( t ).Append( "</i></b>" );
			else if ( run.Bold )
				sb.Append( "<b>" ).Append( t ).Append( "</b>" );
			else if ( run.Italic )
				sb.Append( "<i>" ).Append( t ).Append( "</i>" );
			else
				sb.Append( t );
		}
	}

	static void MergeAdjacentSameFormat( IList<JiMeTextRun> runs )
	{
		for ( var i = 0; i < runs.Count - 1; )
		{
			var a = runs[i];
			var b = runs[i + 1];
			if ( a.IsIconFont == b.IsIconFont && a.Bold == b.Bold && a.Italic == b.Italic )
			{
				a.Text += b.Text;
				runs.RemoveAt( i + 1 );
			}
			else
				i++;
		}
	}
}
