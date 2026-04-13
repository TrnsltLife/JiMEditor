using JiME.TextFormat;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public class JiMePseudoHtmlFormatTests
{
	[Fact]
	public void RoundTrip_Empty()
	{
		Assert.Equal( "", JiMePseudoHtmlFormat.Serialize( JiMePseudoHtmlFormat.Parse( null ) ) );
		Assert.Equal( "", JiMePseudoHtmlFormat.Serialize( JiMePseudoHtmlFormat.Parse( "" ) ) );
	}

	[Fact]
	public void RoundTrip_Plain()
	{
		const string s = "Hello world";
		Assert.Equal( s, JiMePseudoHtmlFormat.NormalizePseudoHtml( s ) );
	}

	[Fact]
	public void Normalize_Escapes_Bare_Angles_And_Ampersand()
	{
		Assert.Equal( "A &lt; B &amp; C &gt; D", JiMePseudoHtmlFormat.NormalizePseudoHtml( "A < B & C > D" ) );
	}

	[Fact]
	public void Normalize_Does_Not_DoubleEncode_Existing_Entities()
	{
		Assert.Equal( "A &lt; B &amp; C &gt; D", JiMePseudoHtmlFormat.NormalizePseudoHtml( "A &lt; B &amp; C &gt; D" ) );
	}

	[Fact]
	public void Normalize_Preserves_Valid_Tags_And_Escapes_Text_Around_Them()
	{
		var s = "<b>hi</b> & <i>there</i> <font=\"Icon\">M</font> <x>";
		Assert.Equal( "<b>hi</b> &amp; <i>there</i> <font=\"Icon\">M</font> &lt;x&gt;", JiMePseudoHtmlFormat.NormalizePseudoHtml( s ) );
	}

	[Fact]
	public void RoundTrip_Paragraphs()
	{
		var s = "First para" + JiMePseudoHtmlFormat.ParagraphSeparator + "Second";
		Assert.Equal( s, JiMePseudoHtmlFormat.NormalizePseudoHtml( s ) );
	}

	[Fact]
	public void RoundTrip_BoldItalicIcon()
	{
		var s = "x <b>bold</b> <i>it</i> <font=\"Icon\">M</font> <font=\"Icon\"><b><i>z</i></b></font> end";
		Assert.Equal( s, JiMePseudoHtmlFormat.NormalizePseudoHtml( s ) );
	}

	[Fact]
	public void Serialize_Matches_Wpf_Nesting_Order()
	{
		var doc = JiMePseudoHtmlFormat.Parse( "" );
		doc.Paragraphs.Clear();
		var p = new JiMeTextParagraph();
		p.Runs.Add( new JiMeTextRun { Text = "z", IsIconFont = true, Bold = true, Italic = true } );
		doc.Paragraphs.Add( p );
		Assert.Equal( "<font=\"Icon\"><b><i>z</i></b></font>", JiMePseudoHtmlFormat.Serialize( doc ) );
	}

	[Fact]
	public void Normalize_Repairs_Interleaved_BoldItalic()
	{
		Assert.Equal( "<b><i>x</i></b>", JiMePseudoHtmlFormat.NormalizePseudoHtml( "<b><i>x</b></i>" ) );
	}

	[Fact]
	public void Normalize_Closes_Unterminated_Tags_At_End()
	{
		Assert.Equal( "<b><i>x</i></b>", JiMePseudoHtmlFormat.NormalizePseudoHtml( "<b><i>x" ) );
	}

	[Fact]
	public void Normalize_Strips_NonIcon_Font_Opens()
	{
		Assert.Equal( "hi", JiMePseudoHtmlFormat.NormalizePseudoHtml( "<font>hi</font>" ) );
		Assert.Equal( "hi", JiMePseudoHtmlFormat.NormalizePseudoHtml( "<font color=\"red\">hi</font>" ) );
	}

	[Fact]
	public void Normalize_Icon_BoldItalic_Interleaved_To_Canonical()
	{
		Assert.Equal(
			"<font=\"Icon\"><b><i>x</i></b></font>",
			JiMePseudoHtmlFormat.NormalizePseudoHtml( "<font=\"Icon\"><b><i>x</b></i></font>" ) );
	}

	[Fact]
	public void ContentSelection_Inside_Bold_Tags_Maps_To_Text_Only()
	{
		const string s = "<b>hi</b>";
		Assert.True( JiMePseudoHtmlContentEditing.TryGetContentRangeForSourceSelection( s, 3, 5, out var c0, out var c1 ) );
		Assert.Equal( 0, c0 );
		Assert.Equal( 2, c1 );
	}

	[Fact]
	public void ToggleBold_AllBold_Then_Unbold()
	{
		var doc = JiMePseudoHtmlFormat.Parse( "<b>hi</b>" );
		JiMePseudoHtmlContentEditing.ApplyBoldToggle( doc, 0, 2 );
		Assert.Equal( "hi", JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) ) );
	}

	[Fact]
	public void ToggleBold_NotBold_Then_Bold()
	{
		var doc = JiMePseudoHtmlFormat.Parse( "hi" );
		JiMePseudoHtmlContentEditing.ApplyBoldToggle( doc, 0, 2 );
		Assert.Equal( "<b>hi</b>", JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) ) );
	}

	[Fact]
	public void ClearFormatting_Inside_Icon_Tag_Removes_Icon()
	{
		const string t = "<font=\"Icon\">M</font>";
		// "M" is the single character immediately after `<font="Icon">` (length 13).
		Assert.True( JiMePseudoHtmlContentEditing.TryGetContentRangeForSourceSelection( t, 13, 14, out var c0, out var c1 ) );
		var doc = JiMePseudoHtmlFormat.Parse( t );
		JiMePseudoHtmlContentEditing.ClearFormattingInContentRange( doc, c0, c1 );
		Assert.Equal( "M", JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) ) );
	}

	[Fact]
	public void SourceRange_RoundTrip_After_Bold_Toggle()
	{
		const string t = "<b>hi</b>";
		Assert.True( JiMePseudoHtmlContentEditing.TryGetContentRangeForSourceSelection( t, 3, 5, out var c0, out var c1 ) );
		var doc = JiMePseudoHtmlFormat.Parse( t );
		JiMePseudoHtmlContentEditing.ApplyBoldToggle( doc, c0, c1 );
		var norm = JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) );
		Assert.Equal( "hi", norm );
		Assert.True( JiMePseudoHtmlContentEditing.TryGetSourceRangeForContentRange( norm, c0, c1, out var s0, out var s1 ) );
		Assert.Equal( 0, s0 );
		Assert.Equal( 2, s1 );
	}

	[Fact]
	public void SelectionRestore_BoldOff_SelectingInnerText_MatchesPlainSpan()
	{
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				"<b>hi</b>",
				3,
				5,
				JiMePseudoHtmlContentEditing.ApplyBoldToggle,
				out var after,
				out var ns,
				out var ne ) );
		Assert.Equal( "hi", after );
		Assert.Equal( 0, ns );
		Assert.Equal( 2, ne );
	}

	[Fact]
	public void SelectionRestore_BoldOn_PlainText_SelectsInnerLettersInMarkup()
	{
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				"hi",
				0,
				2,
				JiMePseudoHtmlContentEditing.ApplyBoldToggle,
				out var after,
				out var ns,
				out var ne ) );
		Assert.Equal( "<b>hi</b>", after );
		Assert.Equal( 3, ns );
		Assert.Equal( 5, ne );
	}

	[Fact]
	public void SelectionRestore_ItalicOn_PlainText_SelectsInnerLettersInMarkup()
	{
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				"ab",
				0,
				2,
				JiMePseudoHtmlContentEditing.ApplyItalicToggle,
				out var after,
				out var ns,
				out var ne ) );
		Assert.Equal( "<i>ab</i>", after );
		Assert.Equal( 3, ns );
		Assert.Equal( 5, ne );
	}

	[Fact]
	public void SelectionRestore_ClearIcon_SelectingGlyph_SelectsPlainGlyph()
	{
		const string t = "<font=\"Icon\">M</font>";
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				t,
				13,
				14,
				JiMePseudoHtmlContentEditing.ClearFormattingInContentRange,
				out var after,
				out var ns,
				out var ne ) );
		Assert.Equal( "M", after );
		Assert.Equal( 0, ns );
		Assert.Equal( 1, ne );
	}

	[Fact]
	public void SelectionRestore_PartialBoldOff_SelectsMiddlePlainRun()
	{
		// `<b>hello</b>` — source "ell" is indices 4–7 (e,l,l).
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				"<b>hello</b>",
				4,
				7,
				JiMePseudoHtmlContentEditing.ApplyBoldToggle,
				out var after,
				out var ns,
				out var ne ) );
		Assert.Equal( "<b>h</b>ell<b>o</b>", after );
		Assert.Equal( 8, ns );
		Assert.Equal( 11, ne );
		Assert.Equal( "ell", after.Substring( ns, ne - ns ) );
	}

	[Theory]
	[InlineData( "<b>hi</b>", 0, 2, 3, 5 )]
	[InlineData( "<i>x</i>", 0, 1, 3, 4 )]
	public void TryGetSourceRangeForContentRange_FindsLiteralSpan( string markup, int c0, int c1, int expStart, int expEnd )
	{
		Assert.True( JiMePseudoHtmlContentEditing.TryGetSourceRangeForContentRange( markup, c0, c1, out var s0, out var s1 ) );
		Assert.Equal( expStart, s0 );
		Assert.Equal( expEnd, s1 );
	}

	[Fact]
	public void IconFontToggle_OnPlain_Wraps_Then_Toggle_Removes()
	{
		Assert.True(
			JiMePseudoHtmlContentEditing.TryGetMarkupAndSelectionAfterSemanticEdit(
				"z",
				0,
				1,
				JiMePseudoHtmlContentEditing.ApplyIconFontToggle,
				out var wrapped,
				out _,
				out _ ) );
		Assert.Equal( "<font=\"Icon\">z</font>", wrapped );

		var doc = JiMePseudoHtmlFormat.Parse( wrapped );
		JiMePseudoHtmlContentEditing.ApplyIconFontToggle( doc, 0, 1 );
		Assert.Equal( "z", JiMePseudoHtmlFormat.NormalizePseudoHtml( JiMePseudoHtmlFormat.Serialize( doc ) ) );
	}
}
