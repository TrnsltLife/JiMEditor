// Avalonia preview for JiME pseudo-HTML (2026-04-05)
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using JiME.TextFormat;

namespace JiMEditor.Avalonia;

public static class JiMeAvaloniaRichPreview
{
	public const string IconFontFamilyUri = "avares://JiMEditor.Avalonia/Assets/LoTR-JiME-Icons.ttf#LoTR JiME Icons";

	public static readonly FontFamily IconFontFamily = new( IconFontFamilyUri );

	public static void Apply( TextBlock textBlock, JiMeTextDocument doc, FontFamily? bodyFont = null )
	{
		bodyFont ??= textBlock.FontFamily ?? FontFamily.Default;
		textBlock.Text = null;
		textBlock.Inlines.Clear();

		for ( var pi = 0; pi < doc.Paragraphs.Count; pi++ )
		{
			var para = doc.Paragraphs[pi];
			foreach ( var run in para.Runs )
			{
				if ( string.IsNullOrEmpty( run.Text ) )
					continue;
				var r = new Run( DecodeBasicEntities( run.Text ) )
				{
					FontFamily = run.IsIconFont ? IconFontFamily : bodyFont,
					FontWeight = run.Bold ? FontWeight.Bold : FontWeight.Normal,
					FontStyle = run.Italic ? FontStyle.Italic : FontStyle.Normal,
					Foreground = textBlock.Foreground
				};
				textBlock.Inlines.Add( r );
			}

			if ( pi < doc.Paragraphs.Count - 1 )
			{
				textBlock.Inlines.Add( new LineBreak() );
				textBlock.Inlines.Add( new LineBreak() );
			}
		}
	}

	static string DecodeBasicEntities( string s )
	{
		// Keep decode scope narrow to JiME needs; do not decode quotes or arbitrary entities here.
		return s
			.Replace( "&lt;", "<" )
			.Replace( "&gt;", ">" )
			.Replace( "&amp;", "&" );
	}
}
