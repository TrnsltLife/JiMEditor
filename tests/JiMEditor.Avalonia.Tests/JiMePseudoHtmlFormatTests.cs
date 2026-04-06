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
}
