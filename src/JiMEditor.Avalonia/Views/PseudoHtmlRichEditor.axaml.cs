// Source + live preview for JiME pseudo-HTML (2026-04-05)
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class PseudoHtmlRichEditor : UserControl
{
	static readonly string[][] IconButtonRows =
	{
		new[] { "M", "Z", "A", "S", "W", "X", "s", "f", "u", "a", "O", "T", "m", "R", "t", "D", "F", "C", "r", "v", "p", "d", "w", "c", "I", "L" },
		new[] { "B", "b", "l", "x", "i", "@", "!", "e", "g", "k", "H", "h", "K", "E", "(", ")", "-", "/" }
	};

	public PseudoHtmlRichEditor()
	{
		InitializeComponent();
		IconToolbarRows.ItemsSource = IconButtonRows;
	}

	public string Text
	{
		get => SourceBox.Text ?? "";
		set
		{
			SourceBox.Text = value ?? "";
			UpdatePreview();
		}
	}

	void SourceBox_OnTextChanged( object? sender, TextChangedEventArgs e ) => UpdatePreview();

	/// <summary>Parse+serialize repairs (tag order, interleaving, unterminated tags, non-Icon font opens).</summary>
	void ApplyNormalizedText( string raw, int caretHint )
	{
		var oldLen = raw.Length;
		var norm = JiMePseudoHtmlFormat.NormalizePseudoHtml( raw );
		SourceBox.Text = norm;
		var diff = norm.Length - oldLen;
		SourceBox.CaretIndex = Math.Clamp( caretHint + diff, 0, norm.Length );
		UpdatePreview();
	}

	void UpdatePreview()
	{
		var doc = JiMePseudoHtmlFormat.Parse( SourceBox.Text );
		JiMeAvaloniaRichPreview.Apply( PreviewBlock, doc );
		PreviewBlock.Opacity = 1;
	}

	protected override void OnLoaded( RoutedEventArgs e )
	{
		base.OnLoaded( e );
		UpdatePreview();
	}

	void Bold_Click( object? sender, RoutedEventArgs e ) =>
		ToggleFormatOrWrapEmpty( "<b>", "</b>", JiMePseudoHtmlContentEditing.ApplyBoldToggle );

	void Italic_Click( object? sender, RoutedEventArgs e ) =>
		ToggleFormatOrWrapEmpty( "<i>", "</i>", JiMePseudoHtmlContentEditing.ApplyItalicToggle );

	void ClearFmt_Click( object? sender, RoutedEventArgs e )
	{
		var (start, end) = GetSelectionRange();
		var t = SourceBox.Text ?? "";
		if ( start >= end )
			return;
		if ( !JiMePseudoHtmlContentEditing.TryGetContentRangeForSourceSelection( t, start, end, out var c0, out var c1 ) )
			return;
		var doc = JiMePseudoHtmlFormat.Parse( t );
		JiMePseudoHtmlContentEditing.ClearFormattingInContentRange( doc, c0, c1 );
		ApplySemanticEdit( doc, c0, c1 );
	}

	void IconGlyph_Click( object? sender, RoutedEventArgs e )
	{
		if ( sender is not Button { Content: string ch } || string.IsNullOrEmpty( ch ) )
			return;
		var insert = "<font=\"Icon\">" + ch + "</font>";
		InsertAtCaret( insert );
	}

	delegate void ApplySpanToggle( JiMeTextDocument doc, int cStart, int cEnd );

	void ToggleFormatOrWrapEmpty( string open, string close, ApplySpanToggle toggleSpan )
	{
		var (start, end) = GetSelectionRange();
		var t = SourceBox.Text ?? "";
		if ( start == end )
		{
			var caret = start < 0 || start > t.Length ? t.Length : start;
			var inserted = t.Insert( caret, open + close );
			ApplyNormalizedText( inserted, caret + open.Length );
			return;
		}

		if ( !JiMePseudoHtmlContentEditing.TryGetContentRangeForSourceSelection( t, start, end, out var c0, out var c1 ) )
		{
			var sel = t.Substring( start, end - start );
			var wrapped = open + sel + close;
			var wrappedDoc = t[..start] + wrapped + t[end..];
			ApplyNormalizedText( wrappedDoc, start + wrapped.Length );
			return;
		}

		var doc = JiMePseudoHtmlFormat.Parse( t );
		toggleSpan( doc, c0, c1 );
		ApplySemanticEdit( doc, c0, c1 );
	}

	void ApplySemanticEdit( JiMeTextDocument doc, int cStart, int cEnd )
	{
		var raw = JiMePseudoHtmlFormat.Serialize( doc );
		var newText = JiMePseudoHtmlFormat.NormalizePseudoHtml( raw );
		if ( JiMePseudoHtmlContentEditing.TryGetSourceRangeForContentRange( newText, cStart, cEnd, out var ns, out var ne ) )
		{
			SourceBox.Text = newText;
			SourceBox.SelectionStart = Math.Min( ns, ne );
			SourceBox.SelectionEnd = Math.Max( ns, ne );
			SourceBox.CaretIndex = ne;
		}
		else
			ApplyNormalizedText( newText, newText.Length );

		UpdatePreview();
	}

	void InsertAtCaret( string insert )
	{
		var t = SourceBox.Text ?? "";
		var caret = SourceBox.CaretIndex;
		if ( caret < 0 || caret > t.Length )
			caret = t.Length;
		var combined = t.Insert( caret, insert );
		ApplyNormalizedText( combined, caret + insert.Length );
	}

	(int start, int end) GetSelectionRange()
	{
		var t = SourceBox.Text ?? "";
		var a = SourceBox.SelectionStart;
		var b = SourceBox.SelectionEnd;
		var start = Math.Min( a, b );
		var end = Math.Max( a, b );
		start = Math.Clamp( start, 0, t.Length );
		end = Math.Clamp( end, 0, t.Length );
		return ( start, end );
	}
}
