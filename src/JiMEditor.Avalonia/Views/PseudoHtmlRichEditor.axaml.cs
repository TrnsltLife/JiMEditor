// Source + live preview for JiME pseudo-HTML (2026-04-05)
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class PseudoHtmlRichEditor : UserControl
{
	public static readonly StyledProperty<string?> WatermarkProperty =
		AvaloniaProperty.Register<PseudoHtmlRichEditor, string?>(
			nameof( Watermark ),
			"" /*default to an empty watermark*/ );

	static readonly string[][][] IconButtonRows =
	{
		new[]
		{
			new[] { "M", "Z", "A", "S", "W", "X" },
			new[] { "I", "L" },
			new[] { "a", "O", "T", "m", "R", "t" },
			new[] { "r", "v", "p", "d", "w", "c" },
			new[] { "B", "b", "l" }
		},
		new[]
		{
			new[] { "D", "F", "C" },
			new[] { "s", "f", "u" },
			new[] { "H", "h", "K", "E" },
			new[] { "x", "i", "@", "!", "e", "g", "k" },
			new[] { "(", ")", "-", "/" }
		}
	};

	public PseudoHtmlRichEditor()
	{
		InitializeComponent();
		IconToolbarRows.ItemsSource = IconButtonRows;
		SourceBox.AddHandler( InputElement.KeyDownEvent, SourceBox_OnKeyDown, RoutingStrategies.Tunnel );
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

	public string? Watermark
	{
		get => GetValue( WatermarkProperty );
		set => SetValue( WatermarkProperty, value );
	}

	void SourceBox_OnTextChanged( object? sender, TextChangedEventArgs e ) => UpdatePreview();

	void SourceBox_OnKeyDown( object? sender, KeyEventArgs e )
	{
		if ( !e.KeyModifiers.HasFlag( KeyModifiers.Control ) )
			return;
		switch ( e.Key )
		{
			case Key.B:
				e.Handled = true;
				ToggleFormatOrWrapEmpty( "<b>", "</b>", JiMePseudoHtmlContentEditing.ApplyBoldToggle );
				break;
			case Key.I:
				e.Handled = true;
				ToggleFormatOrWrapEmpty( "<i>", "</i>", JiMePseudoHtmlContentEditing.ApplyItalicToggle );
				break;
			case Key.Space:
				e.Handled = true;
				ApplyClearFormatting();
				break;
			case Key.F:
				e.Handled = true;
				ToggleFormatOrWrapEmpty( "<font=\"Icon\">", "</font>", JiMePseudoHtmlContentEditing.ApplyIconFontToggle );
				break;
		}
	}

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

	void ClearFmt_Click( object? sender, RoutedEventArgs e ) => ApplyClearFormatting();

	void ApplyClearFormatting()
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
		if ( sender is not Button { Content: string ch } || string.IsNullOrWhiteSpace( ch ) )
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
			var selLo = Math.Clamp( Math.Min( ns, ne ), 0, newText.Length );
			var selHi = Math.Clamp( Math.Max( ns, ne ), 0, newText.Length );
			SourceBox.Text = newText;
			// Setting CaretIndex after a range collapses selection in Avalonia; defer range restore so it survives the Text change.
			Dispatcher.UIThread.Post(
				() =>
				{
					if ( SourceBox.Text != newText )
						return;
					SourceBox.SelectionStart = selLo;
					SourceBox.SelectionEnd = selHi;
					if ( selLo == selHi )
						SourceBox.CaretIndex = selLo;
				},
				DispatcherPriority.Normal );
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
