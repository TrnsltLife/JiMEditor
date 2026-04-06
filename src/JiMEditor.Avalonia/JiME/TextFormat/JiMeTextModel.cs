// Ported concept from: JiMEditor RichTextBoxIconEditor pseudo-HTML (2026-04-05)
using System.Collections.Generic;

namespace JiME.TextFormat;

/// <summary>One styled span inside a paragraph (maps to pseudo-HTML runs).</summary>
public sealed class JiMeTextRun
{
	public string Text { get; set; } = "";
	public bool IsIconFont { get; set; }
	public bool Bold { get; set; }
	public bool Italic { get; set; }
}

/// <summary>Paragraph separated from others by \r\n\r\n in stored strings.</summary>
public sealed class JiMeTextParagraph
{
	public List<JiMeTextRun> Runs { get; } = new();
}

/// <summary>Full story / rich-text document for JiME pseudo-HTML.</summary>
public sealed class JiMeTextDocument
{
	public List<JiMeTextParagraph> Paragraphs { get; } = new();

	public static JiMeTextDocument Empty()
	{
		var d = new JiMeTextDocument();
		d.Paragraphs.Add( new JiMeTextParagraph() );
		return d;
	}
}
