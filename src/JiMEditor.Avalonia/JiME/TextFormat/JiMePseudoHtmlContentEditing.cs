// Map markup source indices ↔ logical text (content) indices for selection-aware edits (2026-04-06).
using System;
using System.Collections.Generic;

namespace JiME.TextFormat;

public static class JiMePseudoHtmlContentEditing
{
	const string FontIconOpen = "<font=\"Icon\">";

	enum WalkFmtTag
	{
		IconFont,
		Bold,
		Italic
	}

	sealed class SourceToContentWalkState
	{
		public int GlobalCi;
		public int? MinSelC;
		public int MaxSelExclusive;
		public int SelStart;
		public int SelEnd;

		public void OnLiteralChar( int srcIndex )
		{
			if ( srcIndex >= SelStart && srcIndex < SelEnd )
			{
				MinSelC = MinSelC.HasValue ? Math.Min( MinSelC.Value, GlobalCi ) : GlobalCi;
				MaxSelExclusive = Math.Max( MaxSelExclusive, GlobalCi + 1 );
			}

			GlobalCi++;
		}
	}

	sealed class ContentToSourceWalkState
	{
		public int GlobalCi;
		public int? SrcStart;
		public int SrcEndExclusive;
		public int CStart;
		public int CEnd;

		public void OnLiteralChar( int srcIndex )
		{
			if ( GlobalCi >= CStart && GlobalCi < CEnd )
			{
				if ( SrcStart == null )
					SrcStart = srcIndex;
				SrcEndExclusive = srcIndex + 1;
			}

			GlobalCi++;
		}
	}

	static void CloseTagRepairingWalk( List<WalkFmtTag> stack, WalkFmtTag expected )
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

	/// <summary>
	/// Maps a substring of the stored markup (selection in TextBox) to half-open content indices [cStart, cEnd).
	/// Ignores paragraph separators and tag markup; only literal text characters count.
	/// </summary>
	public static bool TryGetContentRangeForSourceSelection( string html, int selStart, int selEnd, out int cStart, out int cEnd )
	{
		cStart = cEnd = 0;
		if ( string.IsNullOrEmpty( html ) || selStart >= selEnd )
			return false;
		selStart = Math.Clamp( selStart, 0, html.Length );
		selEnd = Math.Clamp( selEnd, 0, html.Length );
		if ( selStart >= selEnd )
			return false;

		var acc = new SourceToContentWalkState { SelStart = selStart, SelEnd = selEnd };
		var i = 0;
		while ( i < html.Length )
		{
			if ( TryConsumeParagraphSeparator( html, ref i ) )
				continue;

			var paraEnd = html.IndexOf( JiMePseudoHtmlFormat.ParagraphSeparator, i, StringComparison.Ordinal );
			if ( paraEnd < 0 )
				paraEnd = html.Length;

			WalkParagraphLiterals( html, i, paraEnd, acc );

			i = paraEnd;
		}

		if ( acc.MinSelC == null )
			return false;
		cStart = acc.MinSelC.Value;
		cEnd = acc.MaxSelExclusive;
		return cStart < cEnd;
	}

	/// <summary>
	/// After serialize/normalize, maps content range back to source indices for selection restore.
	/// </summary>
	public static bool TryGetSourceRangeForContentRange( string html, int cStart, int cEnd, out int srcStart, out int srcEnd )
	{
		srcStart = srcEnd = 0;
		if ( string.IsNullOrEmpty( html ) || cStart >= cEnd )
			return false;

		var acc = new ContentToSourceWalkState { CStart = cStart, CEnd = cEnd };
		var i = 0;
		while ( i < html.Length )
		{
			if ( TryConsumeParagraphSeparator( html, ref i ) )
				continue;

			var paraEnd = html.IndexOf( JiMePseudoHtmlFormat.ParagraphSeparator, i, StringComparison.Ordinal );
			if ( paraEnd < 0 )
				paraEnd = html.Length;

			MapParagraphLiteralsToSource( html, i, paraEnd, acc );

			i = paraEnd;
		}

		if ( acc.SrcStart == null )
			return false;
		srcStart = acc.SrcStart.Value;
		srcEnd = acc.SrcEndExclusive;
		return srcStart < srcEnd;
	}

	/// <summary>
	/// Runs the same mapping pipeline as the rich editor after B/I/Clear: content selection → edit → normalize → restored source span.
	/// Used by tests and to validate selection restoration without UI.
	/// </summary>
	public static bool TryGetMarkupAndSelectionAfterSemanticEdit(
		string markupBefore,
		int sourceSelStart,
		int sourceSelEnd,
		Action<JiMeTextDocument, int, int> applyEdit,
		out string markupAfter,
		out int newSourceSelStart,
		out int newSourceSelEnd )
	{
		markupAfter = markupBefore;
		newSourceSelStart = newSourceSelEnd = 0;
		if ( !TryGetContentRangeForSourceSelection( markupBefore, sourceSelStart, sourceSelEnd, out var c0, out var c1 ) )
			return false;
		var doc = JiMePseudoHtmlFormat.Parse( markupBefore );
		applyEdit( doc, c0, c1 );
		markupAfter = JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) );
		return TryGetSourceRangeForContentRange( markupAfter, c0, c1, out newSourceSelStart, out newSourceSelEnd );
	}

	public static void ApplyBoldToggle( JiMeTextDocument doc, int cStart, int cEnd )
	{
		if ( cStart >= cEnd )
			return;
		SplitAtContentOffset( doc, cEnd );
		SplitAtContentOffset( doc, cStart );
		var allBold = AllCharsHaveBold( doc, cStart, cEnd );
		var newBold = !allBold;
		ApplyBoldInRange( doc, cStart, cEnd, newBold );
		MergeAllParagraphs( doc );
	}

	public static void ApplyItalicToggle( JiMeTextDocument doc, int cStart, int cEnd )
	{
		if ( cStart >= cEnd )
			return;
		SplitAtContentOffset( doc, cEnd );
		SplitAtContentOffset( doc, cStart );
		var allItalic = AllCharsHaveItalic( doc, cStart, cEnd );
		var newItalic = !allItalic;
		ApplyItalicInRange( doc, cStart, cEnd, newItalic );
		MergeAllParagraphs( doc );
	}

	public static void ApplyIconFontToggle( JiMeTextDocument doc, int cStart, int cEnd )
	{
		if ( cStart >= cEnd )
			return;
		SplitAtContentOffset( doc, cEnd );
		SplitAtContentOffset( doc, cStart );
		var allIcon = AllCharsHaveIconFont( doc, cStart, cEnd );
		ApplyIconFontInRange( doc, cStart, cEnd, !allIcon );
		MergeAllParagraphs( doc );
	}

	public static void ClearFormattingInContentRange( JiMeTextDocument doc, int cStart, int cEnd )
	{
		if ( cStart >= cEnd )
			return;
		SplitAtContentOffset( doc, cEnd );
		SplitAtContentOffset( doc, cStart );
		StripFormattingInRange( doc, cStart, cEnd );
		MergeAllParagraphs( doc );
	}

	static void WalkParagraphLiterals( string s, int paraStart, int paraEnd, SourceToContentWalkState acc )
	{
		var stack = new List<WalkFmtTag>();
		var i = paraStart;

		while ( i < paraEnd )
		{
			if ( s[i] != '<' )
			{
				acc.OnLiteralChar( i );
				i++;
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, FontIconOpen ) )
			{
				stack.Add( WalkFmtTag.IconFont );
				continue;
			}

			if ( IsNonIconFontOpen( s, i, paraEnd ) )
			{
				SkipThroughClosingAngle( s, ref i, paraEnd );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</font>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.IconFont );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "<b>" ) )
			{
				stack.Add( WalkFmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</b>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "<i>" ) )
			{
				stack.Add( WalkFmtTag.Italic );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</i>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.Italic );
				continue;
			}

			acc.OnLiteralChar( i );
			i++;
		}
	}

	static void MapParagraphLiteralsToSource( string s, int paraStart, int paraEnd, ContentToSourceWalkState acc )
	{
		var stack = new List<WalkFmtTag>();
		var i = paraStart;

		while ( i < paraEnd )
		{
			if ( s[i] != '<' )
			{
				acc.OnLiteralChar( i );
				i++;
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, FontIconOpen ) )
			{
				stack.Add( WalkFmtTag.IconFont );
				continue;
			}

			if ( IsNonIconFontOpen( s, i, paraEnd ) )
			{
				SkipThroughClosingAngle( s, ref i, paraEnd );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</font>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.IconFont );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "<b>" ) )
			{
				stack.Add( WalkFmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</b>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.Bold );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "<i>" ) )
			{
				stack.Add( WalkFmtTag.Italic );
				continue;
			}

			if ( TryConsume( s, ref i, paraEnd, "</i>" ) )
			{
				CloseTagRepairingWalk( stack, WalkFmtTag.Italic );
				continue;
			}

			acc.OnLiteralChar( i );
			i++;
		}
	}

	static bool TryConsumeParagraphSeparator( string s, ref int i )
	{
		var sep = JiMePseudoHtmlFormat.ParagraphSeparator;
		if ( i + sep.Length > s.Length )
			return false;
		if ( string.CompareOrdinal( s, i, sep, 0, sep.Length ) != 0 )
			return false;
		i += sep.Length;
		return true;
	}

	static bool TryConsume( string s, ref int i, int limit, string token )
	{
		if ( i + token.Length > limit )
			return false;
		if ( string.CompareOrdinal( s, i, token, 0, token.Length ) != 0 )
			return false;
		i += token.Length;
		return true;
	}

	static bool IsNonIconFontOpen( string s, int i, int limit )
	{
		if ( i >= limit || s[i] != '<' )
			return false;
		const string prefix = "<font";
		if ( i + prefix.Length > limit )
			return false;
		if ( string.CompareOrdinal( s, i, prefix, 0, prefix.Length ) != 0 )
			return false;
		if ( i + FontIconOpen.Length <= limit && string.CompareOrdinal( s, i, FontIconOpen, 0, FontIconOpen.Length ) == 0 )
			return false;
		return true;
	}

	static void SkipThroughClosingAngle( string s, ref int i, int limit )
	{
		while ( i < limit && s[i] != '>' )
			i++;
		if ( i < limit && s[i] == '>' )
			i++;
	}

	static int GetContentLength( JiMeTextDocument doc )
	{
		var n = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
				n += r.Text?.Length ?? 0;
		}

		return n;
	}

	static void SplitAtContentOffset( JiMeTextDocument doc, int offset )
	{
		if ( offset <= 0 )
			return;
		var total = GetContentLength( doc );
		if ( offset >= total )
			return;
		var (pi, ri, local) = FindRunAtContentOffset( doc, offset );
		if ( local == 0 )
			return;
		var p = doc.Paragraphs[pi];
		var run = p.Runs[ri];
		var rt = run.Text ?? "";
		var tail = rt.Substring( local );
		run.Text = rt.Substring( 0, local );
		p.Runs.Insert(
			ri + 1,
			new JiMeTextRun
			{
				Text = tail,
				IsIconFont = run.IsIconFont,
				Bold = run.Bold,
				Italic = run.Italic
			} );
	}

	static (int pi, int ri, int local) FindRunAtContentOffset( JiMeTextDocument doc, int offset )
	{
		var g = 0;
		for ( var pi = 0; pi < doc.Paragraphs.Count; pi++ )
		{
			var p = doc.Paragraphs[pi];
			for ( var ri = 0; ri < p.Runs.Count; ri++ )
			{
				var len = p.Runs[ri].Text?.Length ?? 0;
				if ( g + len < offset )
				{
					g += len;
					continue;
				}

				return ( pi, ri, offset - g );
			}
		}

		return ( 0, 0, 0 );
	}

	static bool AllCharsHaveBold( JiMeTextDocument doc, int cStart, int cEnd )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 && !r.Bold )
					return false;
				g = rg1;
			}
		}

		return true;
	}

	static bool AllCharsHaveItalic( JiMeTextDocument doc, int cStart, int cEnd )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 && !r.Italic )
					return false;
				g = rg1;
			}
		}

		return true;
	}

	static bool AllCharsHaveIconFont( JiMeTextDocument doc, int cStart, int cEnd )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 && !r.IsIconFont )
					return false;
				g = rg1;
			}
		}

		return true;
	}

	static void ApplyIconFontInRange( JiMeTextDocument doc, int cStart, int cEnd, bool iconFont )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 )
					r.IsIconFont = iconFont;
				g = rg1;
			}
		}
	}

	static void ApplyBoldInRange( JiMeTextDocument doc, int cStart, int cEnd, bool bold )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 )
					r.Bold = bold;
				g = rg1;
			}
		}
	}

	static void ApplyItalicInRange( JiMeTextDocument doc, int cStart, int cEnd, bool italic )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 )
					r.Italic = italic;
				g = rg1;
			}
		}
	}

	static void StripFormattingInRange( JiMeTextDocument doc, int cStart, int cEnd )
	{
		var g = 0;
		foreach ( var p in doc.Paragraphs )
		{
			foreach ( var r in p.Runs )
			{
				var len = r.Text?.Length ?? 0;
				var rg0 = g;
				var rg1 = g + len;
				var o0 = Math.Max( rg0, cStart );
				var o1 = Math.Min( rg1, cEnd );
				if ( o0 < o1 )
				{
					r.IsIconFont = false;
					r.Bold = false;
					r.Italic = false;
				}

				g = rg1;
			}
		}
	}

	static void MergeAllParagraphs( JiMeTextDocument doc )
	{
		foreach ( var p in doc.Paragraphs )
			MergeAdjacentSameFormat( p.Runs );
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
